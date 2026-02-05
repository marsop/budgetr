using Budgetr.Shared.Models;
using System.Text.Json;

namespace Budgetr.Shared.Services;

/// <summary>
/// Implementation of time tracking service with local storage persistence.
/// </summary>
public class TimeTrackingService : ITimeTrackingService
{
    private readonly IStorageService _storage;
    private readonly IMeterConfigurationService _meterConfig;
    private TimeAccount _account = new TimeAccount();
    private const string StorageKey = "budgetr_account";
    
    public TimeAccount Account => _account;

    public TimeSpan TimelinePeriod
    {
        get => _account.TimelinePeriod;
        set
        {
            if (_account.TimelinePeriod != value)
            {
                _account.TimelinePeriod = value;
                OnStateChanged?.Invoke();
                _ = SaveAsync();
            }
        }
    }
    
    public event Action? OnStateChanged;

    public TimeTrackingService(IStorageService storage, IMeterConfigurationService meterConfig)
    {
        _storage = storage;
        _meterConfig = meterConfig;
    }

    public TimeSpan GetCurrentBalance()
    {
        return TimeSpan.FromTicks(_account.Events.Sum(e => e.TimeContribution.Ticks));
    }

    public MeterEvent? GetActiveEvent()
    {
        return _account.Events.FirstOrDefault(e => e.IsActive);
    }

    public void ActivateMeter(Guid meterId)
    {
        // First deactivate any active meter
        DeactivateMeter();
        
        var meter = _account.Meters.FirstOrDefault(m => m.Id == meterId);
        if (meter == null) return;
        
        var newEvent = new MeterEvent
        {
            StartTime = DateTimeOffset.UtcNow,
            Factor = meter.Factor,
            MeterName = meter.Name
        };
        
        _account.Events.Add(newEvent);
        OnStateChanged?.Invoke();
        _ = SaveAsync();
    }

    public void DeactivateMeter()
    {
        var activeEvent = GetActiveEvent();
        if (activeEvent != null)
        {
            activeEvent.EndTime = DateTimeOffset.UtcNow;
            OnStateChanged?.Invoke();
            _ = SaveAsync();
        }
    }

    public void DeleteEvent(Guid eventId)
    {
        var eventToDelete = _account.Events.FirstOrDefault(e => e.Id == eventId);
        if (eventToDelete != null)
        {
            _account.Events.Remove(eventToDelete);
            OnStateChanged?.Invoke();
            _ = SaveAsync();
        }
    }

    public List<TimelineDataPoint> GetTimelineData(TimeSpan period)
    {
        var points = new List<TimelineDataPoint>();
        var endTime = DateTimeOffset.UtcNow;
        var startTime = endTime - period;
        
        // Get all events that overlap with the period
        var relevantEvents = _account.Events
            .Where(e => e.StartTime <= endTime && (e.EndTime ?? endTime) >= startTime)
            .OrderBy(e => e.StartTime)
            .ToList();
        
        // Calculate balance before the period starts
        double runningBalance = 0;
        var eventsBefore = _account.Events
            .Where(e => e.StartTime < startTime)
            .ToList();
        
        foreach (var evt in eventsBefore)
        {
            var effectiveEnd = evt.EndTime ?? startTime;
            if (effectiveEnd > startTime) effectiveEnd = startTime;
            var duration = effectiveEnd - evt.StartTime;
            runningBalance += duration.TotalHours * evt.Factor;
        }
        
        // Always add start point
        points.Add(new TimelineDataPoint { Timestamp = startTime, BalanceHours = runningBalance });
        
        // Add points for each event transition in the period
        foreach (var evt in relevantEvents)
        {
            // Point at start of event (before contribution)
            if (evt.StartTime >= startTime)
            {
                points.Add(new TimelineDataPoint 
                { 
                    Timestamp = evt.StartTime, 
                    BalanceHours = runningBalance 
                });
            }
            
            // Calculate contribution up to end or now
            var effectiveStart = evt.StartTime < startTime ? startTime : evt.StartTime;
            var effectiveEnd = evt.EndTime ?? endTime;
            if (effectiveEnd > endTime) effectiveEnd = endTime;
            
            var contribution = (effectiveEnd - effectiveStart).TotalHours * evt.Factor;
            runningBalance += contribution;
            
            // Point at end of event (or now)
            if (evt.EndTime.HasValue && evt.EndTime.Value <= endTime)
            {
                points.Add(new TimelineDataPoint 
                { 
                    Timestamp = evt.EndTime.Value, 
                    BalanceHours = runningBalance 
                });
            }
        }
        
        // Always add current point
        points.Add(new TimelineDataPoint { Timestamp = endTime, BalanceHours = runningBalance });
        
        return points.OrderBy(p => p.Timestamp).ToList();
    }

    public async Task SaveAsync()
    {
        var json = JsonSerializer.Serialize(_account);
        await _storage.SetItemAsync(StorageKey, json);
    }

    public async Task LoadAsync()
    {
        var json = await _storage.GetItemAsync(StorageKey);
        if (!string.IsNullOrEmpty(json))
        {
            var loaded = JsonSerializer.Deserialize<TimeAccount>(json);
            if (loaded != null)
            {
                _account.Events = loaded.Events;
                _account.Meters = loaded.Meters;
                
                // Ensure TimelinePeriod is valid (handle migration from versions without it)
                if (loaded.TimelinePeriod != TimeSpan.Zero)
                {
                    _account.TimelinePeriod = loaded.TimelinePeriod;
                }
                else
                {
                    _account.TimelinePeriod = TimeSpan.FromHours(24);
                }
            }
        }

        // Only load default meters if none were loaded from storage
        if (_account.Meters == null || _account.Meters.Count == 0)
        {
            _account.Meters = await _meterConfig.LoadMetersAsync();
        }
        
        // Auto-stop any active events whose meter factor no longer exists
        var availableFactors = _account.Meters.Select(m => m.Factor).ToHashSet();
        foreach (var activeEvent in _account.Events.Where(e => e.IsActive))
        {
            if (!availableFactors.Contains(activeEvent.Factor))
            {
                activeEvent.EndTime = DateTimeOffset.UtcNow;
            }
        }
        
        OnStateChanged?.Invoke();
        await SaveAsync();
    }

    public void RenameMeter(Guid meterId, string newName)
    {
        if (string.IsNullOrWhiteSpace(newName) || newName.Length < 1 || newName.Length > 40)
        {
            throw new ArgumentException("Meter name must be between 1 and 40 characters.");
        }

        var meter = _account.Meters.FirstOrDefault(m => m.Id == meterId);
        if (meter == null) return;

        meter.Name = newName.Trim();
        
        // Update active event if this meter is currently running
        var activeEvent = GetActiveEvent();
        if (activeEvent != null && activeEvent.Factor == meter.Factor)
        {
            activeEvent.MeterName = meter.Name;
        }
        
        OnStateChanged?.Invoke();
        _ = SaveAsync();
    }

    public string ExportData()
    {
        var exportData = new BudgetrExportData
        {
            ExportedAt = DateTimeOffset.UtcNow,
            Meters = _account.Meters,
            Events = _account.Events
        };
        
        return JsonSerializer.Serialize(exportData, new JsonSerializerOptions 
        { 
            WriteIndented = true 
        });
    }

    public async Task ImportDataAsync(string json)
    {
        var importData = JsonSerializer.Deserialize<BudgetrExportData>(json);
        
        if (importData == null)
        {
            throw new InvalidOperationException("Invalid import data format.");
        }

        if (importData.Meters == null || importData.Meters.Count == 0)
        {
            throw new InvalidOperationException("Import data must contain at least one meter.");
        }

        // Validate no duplicate factors
        var duplicateFactors = importData.Meters
            .GroupBy(m => m.Factor)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToList();

        if (duplicateFactors.Any())
        {
            throw new InvalidOperationException(
                $"Duplicate meter factors detected: {string.Join(", ", duplicateFactors)}. Each meter must have a unique factor.");
        }

        // Assign display order based on definition order
        for (int i = 0; i < importData.Meters.Count; i++)
        {
            importData.Meters[i].DisplayOrder = i;
        }

        // Replace current data
        _account.Meters = importData.Meters;
        _account.Events = importData.Events ?? new List<MeterEvent>();

        // Auto-stop any active events whose meter factor no longer exists
        var availableFactors = _account.Meters.Select(m => m.Factor).ToHashSet();
        foreach (var activeEvent in _account.Events.Where(e => e.IsActive))
        {
            if (!availableFactors.Contains(activeEvent.Factor))
            {
                activeEvent.EndTime = DateTimeOffset.UtcNow;
            }
        }

        OnStateChanged?.Invoke();
        await SaveAsync();
    }

    public void DeleteMeter(Guid meterId)
    {
        var meter = _account.Meters.FirstOrDefault(m => m.Id == meterId);
        if (meter == null) return;

        var activeEvent = GetActiveEvent();
        if (activeEvent != null && activeEvent.MeterName == meter.Name) // Matching by name as names are unique per definition (though factor is the strict key, UI uses name) - actually `ActivateMeter` stores name. Let's check logic.
        {
             // Checking by ID in account meters is safer if we had ID in event, but event stores name/factor.
             // Let's rely on the meter we just found. 
             // IF the currently active event corresponds to this meter.
             // GetActiveEvent returns an event. Event has MeterName and Factor.
             // Meter has Name and Factor.
             // The most reliable check for "Is Active" as used in Meters.razor is:
             // activeEvent.MeterName == meter.Name
             
             if (activeEvent.MeterName == meter.Name)
             {
                 throw new InvalidOperationException("Cannot delete the currently active meter.");
             }
        }
        
        _account.Meters.Remove(meter);
        OnStateChanged?.Invoke();
        _ = SaveAsync();
    }

    public void AddMeter(string name, double factor)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 1 || name.Length > 40)
        {
            throw new ArgumentException("Meter name must be between 1 and 40 characters.");
        }

        // Check for duplicate factor
        if (_account.Meters.Any(m => Math.Abs(m.Factor - factor) < 0.001))
        {
            throw new ArgumentException($"A meter with factor {factor} already exists.");
        }

        var newMeter = new Meter
        {
            Name = name.Trim(),
            Factor = factor,
            DisplayOrder = _account.Meters.Count > 0 ? _account.Meters.Max(m => m.DisplayOrder) + 1 : 0
        };

        _account.Meters.Add(newMeter);
        OnStateChanged?.Invoke();
        _ = SaveAsync();
    }
}

/// <summary>
/// Data structure for import/export operations.
/// </summary>
public class BudgetrExportData
{
    public DateTimeOffset ExportedAt { get; set; }
    public List<Meter> Meters { get; set; } = new();
    public List<MeterEvent> Events { get; set; } = new();
}
