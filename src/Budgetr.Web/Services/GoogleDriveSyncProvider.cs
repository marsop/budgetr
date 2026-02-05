using Budgetr.Shared.Services;

namespace Budgetr.Web.Services;

/// <summary>
/// Google Drive implementation of ISyncProvider.
/// </summary>
public class GoogleDriveSyncProvider : ISyncProvider
{
    private readonly GoogleDriveService _driveService;
    
    public string Name => "Google Drive";
    public string Description => "Sync your data with Google Drive";
    public string Icon => "☁️";
    
    public bool IsConfigured => !string.IsNullOrEmpty(_driveService.GetConfiguredClientId());

    public GoogleDriveSyncProvider(GoogleDriveService driveService)
    {
        _driveService = driveService;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        return await _driveService.IsSignedInAsync();
    }

    public async Task<bool> AuthenticateAsync()
    {
        return await _driveService.SignInAsync();
    }

    public async Task SignOutAsync()
    {
        await _driveService.SignOutAsync();
    }

    public async Task<string?> DownloadDataAsync()
    {
        return await _driveService.GetLatestBackupContentAsync();
    }

    public async Task UploadDataAsync(string jsonData)
    {
        await _driveService.SaveBackupAsync(jsonData);
    }

    public async Task<DateTimeOffset?> GetLastBackupTimeAsync()
    {
        return await _driveService.GetBackupLastModifiedAsync();
    }
}
