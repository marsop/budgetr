using Budgetr.Shared.Services;
using Microsoft.JSInterop;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Budgetr.Web.Services;

/// <summary>
/// Auto-sync service implementation using Rx.NET for change detection and debouncing.
/// Automatically backs up data to Google Drive when changes are detected.
/// </summary>
public class AutoSyncService : IAutoSyncService
{
    private readonly ITimeTrackingService _timeService;
    private readonly GoogleDriveService _driveService;
    private readonly IStorageService _storage;
    private readonly IJSRuntime _js;
    
    private const string EnabledStorageKey = "budgetr_autosync_enabled";
    private const string LastSyncStorageKey = "budgetr_autosync_lastsync";
    private const int DebounceMilliseconds = 1000;
    
    private readonly Subject<bool> _changeSubject = new();
    private IDisposable? _subscription;
    private bool _isEnabled;
    private DateTimeOffset? _lastSyncTime;
    private AutoSyncStatus _status = AutoSyncStatus.Idle;
    
    public bool IsEnabled => _isEnabled;
    public DateTimeOffset? LastSyncTime => _lastSyncTime;
    public AutoSyncStatus Status => _status;
    
    public event Action<AutoSyncStatus>? OnStatusChanged;

    public AutoSyncService(
        ITimeTrackingService timeService,
        GoogleDriveService driveService,
        IStorageService storage,
        IJSRuntime js)
    {
        _timeService = timeService;
        _driveService = driveService;
        _storage = storage;
        _js = js;
        
        // Subscribe to time service state changes and push to reactive stream
        _timeService.OnStateChanged += OnDataChanged;
    }

    private void OnDataChanged()
    {
        if (_isEnabled)
        {
            _changeSubject.OnNext(true);
        }
    }

    public async Task EnableAsync()
    {
        if (_isEnabled)
            return;
        
        // Ensure GoogleDriveService is initialized (in case Sync.razor used direct JS interop)
        await _driveService.TryAutoInitializeAsync();
            
        // Check if Google Drive is signed in
        var isSignedIn = await _driveService.IsSignedInAsync();
        if (!isSignedIn)
        {
            throw new InvalidOperationException("Please sign in to Google Drive first.");
        }
        
        _isEnabled = true;
        await _storage.SetItemAsync(EnabledStorageKey, "true");
        
        // Load last sync time
        await LoadLastSyncTimeAsync();
        
        // Set up debounced subscription using Rx.NET
        _subscription = _changeSubject
            .Throttle(TimeSpan.FromMilliseconds(DebounceMilliseconds))
            .Subscribe(async _ => await PerformSyncAsync());
        
        UpdateStatus(AutoSyncStatus.Idle);
    }

    public async Task DisableAsync()
    {
        if (!_isEnabled)
            return;
            
        _isEnabled = false;
        await _storage.SetItemAsync(EnabledStorageKey, "false");
        
        // Dispose the subscription
        _subscription?.Dispose();
        _subscription = null;
        
        UpdateStatus(AutoSyncStatus.Idle);
    }

    private async Task PerformSyncAsync()
    {
        if (!_isEnabled)
            return;
            
        try
        {
            UpdateStatus(AutoSyncStatus.Syncing);
            
            // Ensure service is initialized
            await _driveService.TryAutoInitializeAsync();
            
            // Check if still signed in
            var isSignedIn = await _driveService.IsSignedInAsync();
            if (!isSignedIn)
            {
                Console.WriteLine("Auto-sync: Not signed in, disabling auto-sync.");
                await DisableAsync();
                UpdateStatus(AutoSyncStatus.Failed);
                return;
            }
            
            // Export and upload data
            var json = _timeService.ExportData();
            await _driveService.SaveBackupAsync(json);
            
            // Update last sync time
            _lastSyncTime = DateTimeOffset.UtcNow;
            await _storage.SetItemAsync(LastSyncStorageKey, _lastSyncTime.Value.ToString("O"));
            
            UpdateStatus(AutoSyncStatus.Success);
            Console.WriteLine($"Auto-sync: Backup completed at {_lastSyncTime}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Auto-sync failed: {ex.Message}");
            UpdateStatus(AutoSyncStatus.Failed);
        }
    }

    private async Task LoadLastSyncTimeAsync()
    {
        try
        {
            var lastSyncStr = await _storage.GetItemAsync(LastSyncStorageKey);
            if (!string.IsNullOrEmpty(lastSyncStr) && DateTimeOffset.TryParse(lastSyncStr, out var parsed))
            {
                _lastSyncTime = parsed;
            }
        }
        catch
        {
            // Ignore errors loading last sync time
        }
    }

    /// <summary>
    /// Tries to restore auto-sync state from storage.
    /// Should be called on initialization.
    /// </summary>
    public async Task TryRestoreStateAsync()
    {
        try
        {
            var enabled = await _storage.GetItemAsync(EnabledStorageKey);
            if (enabled == "true")
            {
                // Check if Google Drive is still signed in
                var isSignedIn = await _driveService.IsSignedInAsync();
                if (isSignedIn)
                {
                    await EnableAsync();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to restore auto-sync state: {ex.Message}");
        }
    }

    private void UpdateStatus(AutoSyncStatus status)
    {
        _status = status;
        OnStatusChanged?.Invoke(status);
    }

    public void Dispose()
    {
        _subscription?.Dispose();
        _changeSubject.Dispose();
        _timeService.OnStateChanged -= OnDataChanged;
    }
}
