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
    private const int PollingIntervalSeconds = 30;
    
    private readonly Subject<bool> _changeSubject = new();
    private IDisposable? _subscription;
    private IDisposable? _pollingSubscription;
    private bool _isEnabled;
    private bool _isRestoring;
    private DateTimeOffset? _lastSyncTime;
    private DateTimeOffset? _lastKnownRemoteModifiedTime;
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
        // Don't trigger sync if we are currently restoring data from remote
        if (_isEnabled && !_isRestoring)
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
        
        // Initialize last known remote time to avoid immediate restore loop if we just synced
        if (_lastSyncTime.HasValue)
        {
            _lastKnownRemoteModifiedTime = _lastSyncTime;
        }
        else
        {
             // If we have never synced, try to get the current remote time so we only restore *new* changes
             _lastKnownRemoteModifiedTime = await _driveService.GetBackupLastModifiedAsync();
        }

        // Set up debounced subscription using Rx.NET
        _subscription = _changeSubject
            .Throttle(TimeSpan.FromMilliseconds(DebounceMilliseconds))
            .Subscribe(async _ => await PerformSyncAsync());
            
        // Set up polling for remote changes
        _pollingSubscription = Observable.Interval(TimeSpan.FromSeconds(PollingIntervalSeconds))
            .Subscribe(async _ => await CheckForRemoteChangesAsync());
        
        UpdateStatus(AutoSyncStatus.Idle);
    }

    public async Task DisableAsync()
    {
        if (!_isEnabled)
            return;
            
        _isEnabled = false;
        await _storage.SetItemAsync(EnabledStorageKey, "false");
        
        // Dispose subscriptions
        _subscription?.Dispose();
        _subscription = null;
        
        _pollingSubscription?.Dispose();
        _pollingSubscription = null;
        
        UpdateStatus(AutoSyncStatus.Idle);
    }

    private async Task PerformSyncAsync()
    {
        if (!_isEnabled || _isRestoring)
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
            var modifiedTime = await _driveService.SaveBackupAsync(json);
            
            // Update last sync time and remote modified time
            _lastSyncTime = DateTimeOffset.UtcNow;
            if (modifiedTime.HasValue)
            {
                _lastKnownRemoteModifiedTime = modifiedTime;
            }
            
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
    
    private async Task CheckForRemoteChangesAsync()
    {
        if (!_isEnabled || _isRestoring)
            return;
            
        try
        {
            // Ensure service is initialized
            await _driveService.TryAutoInitializeAsync();
            
            if (!await _driveService.IsSignedInAsync())
                return;

            var remoteModified = await _driveService.GetBackupLastModifiedAsync();
            
            // If remote file exists and is newer than what we last knew about
            if (remoteModified.HasValue && 
                (!_lastKnownRemoteModifiedTime.HasValue || remoteModified.Value > _lastKnownRemoteModifiedTime.Value + TimeSpan.FromSeconds(1)))
            {
                Console.WriteLine($"Auto-sync: Detected remote change. Local: {_lastKnownRemoteModifiedTime}, Remote: {remoteModified}");
                await RestoreDataAsync(remoteModified.Value);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Auto-sync polling failed: {ex.Message}");
        }
    }
    
    private async Task RestoreDataAsync(DateTimeOffset remoteModifiedTime)
    {
        try
        {
            _isRestoring = true;
            UpdateStatus(AutoSyncStatus.Syncing);
            
            var content = await _driveService.GetLatestBackupContentAsync();
            if (!string.IsNullOrEmpty(content))
            {
                await _timeService.ImportDataAsync(content);
                _lastKnownRemoteModifiedTime = remoteModifiedTime;
                _lastSyncTime = DateTimeOffset.UtcNow;
                await _storage.SetItemAsync(LastSyncStorageKey, _lastSyncTime.Value.ToString("O"));
                
                UpdateStatus(AutoSyncStatus.Success);
                Console.WriteLine("Auto-sync: Data restored from Google Drive successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Auto-sync restore failed: {ex.Message}");
            UpdateStatus(AutoSyncStatus.Failed);
        }
        finally
        {
            _isRestoring = false;
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
        _pollingSubscription?.Dispose();
        _changeSubject.Dispose();
        _timeService.OnStateChanged -= OnDataChanged;
    }
}
