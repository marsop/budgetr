using Budgetr.Shared.Models;

namespace Budgetr.Shared.Services;

/// <summary>
/// Interface for time tracking operations.
/// </summary>
public interface ITimeTrackingService
{
    /// <summary>
    /// Gets the current time account state.
    /// </summary>
    TimeAccount Account { get; }
    
    /// <summary>
    /// Gets the current balance, calculated in real-time.
    /// </summary>
    TimeSpan GetCurrentBalance();
    
    /// <summary>
    /// Gets the currently active meter event, if any.
    /// </summary>
    MeterEvent? GetActiveEvent();
    
    /// <summary>
    /// Activates a meter by its ID. Deactivates any currently active meter first.
    /// </summary>
    void ActivateMeter(Guid meterId);
    
    /// <summary>
    /// Deactivates the currently active meter.
    /// </summary>
    void DeactivateMeter();
    
    /// <summary>
    /// Gets timeline data points for the specified period.
    /// </summary>
    List<TimelineDataPoint> GetTimelineData(TimeSpan period);
    
    /// <summary>
    /// Deletes an event by its ID. Updates balance and triggers state change.
    /// </summary>
    void DeleteEvent(Guid eventId);
    
    /// <summary>
    /// Saves the current state to persistent storage.
    /// </summary>
    Task SaveAsync();
    
    /// <summary>
    /// Loads the state from persistent storage.
    /// </summary>
    Task LoadAsync();
    
    /// <summary>
    /// Event raised when the account state changes.
    /// </summary>
    event Action? OnStateChanged;
    
    /// <summary>
    /// Exports all data (meters and events) as a JSON string.
    /// </summary>
    string ExportData();
    
    /// <summary>
    /// Imports data from a JSON string, replacing current meters and events.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when import data has duplicate meter factors.</exception>
    Task ImportDataAsync(string json);
}

/// <summary>
/// Represents a point on the timeline graph.
/// </summary>
public class TimelineDataPoint
{
    public DateTimeOffset Timestamp { get; set; }
    public double BalanceHours { get; set; }
}
