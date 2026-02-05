using Budgetr.Shared.Services;
using Microsoft.JSInterop;

namespace Budgetr.Web.Services;

/// <summary>
/// Web implementation of IGoogleDriveService using JavaScript interop.
/// </summary>
public class GoogleDriveService : IGoogleDriveService
{
    private readonly IJSRuntime _js;
    private readonly IStorageService _storage;
    private const string ClientIdStorageKey = "budgetr_gdrive_clientid";
    private string? _clientId;
    private bool _isInitialized;

    public GoogleDriveService(IJSRuntime js, IStorageService storage)
    {
        _js = js;
        _storage = storage;
    }

    public async Task InitializeAsync(string clientId)
    {
        if (_isInitialized && _clientId == clientId)
            return;
            
        _clientId = clientId;
        await _storage.SetItemAsync(ClientIdStorageKey, clientId);
        
        try
        {
            await _js.InvokeVoidAsync("googleDriveInterop.initialize", clientId);
            _isInitialized = true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize Google Drive: {ex.Message}");
            throw;
        }
    }

    public async Task<bool> TryAutoInitializeAsync()
    {
        if (_isInitialized)
            return true;
            
        var savedClientId = await _storage.GetItemAsync(ClientIdStorageKey);
        if (!string.IsNullOrEmpty(savedClientId))
        {
            try
            {
                await InitializeAsync(savedClientId);
                return true;
            }
            catch
            {
                return false;
            }
        }
        return false;
    }

    public string? GetConfiguredClientId() => _clientId;

    public async Task<bool> IsSignedInAsync()
    {
        if (!_isInitialized)
            return false;
            
        try
        {
            return await _js.InvokeAsync<bool>("googleDriveInterop.isSignedIn");
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> GetUserEmailAsync()
    {
        if (!_isInitialized)
            return null;
            
        try
        {
            return await _js.InvokeAsync<string?>("googleDriveInterop.getUserEmail");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> SignInAsync()
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Google Drive service not initialized. Please configure Client ID first.");
            
        try
        {
            return await _js.InvokeAsync<bool>("googleDriveInterop.signIn");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sign in failed: {ex.Message}");
            throw;
        }
    }

    public async Task SignOutAsync()
    {
        if (!_isInitialized)
            return;
            
        try
        {
            await _js.InvokeVoidAsync("googleDriveInterop.signOut");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Sign out failed: {ex.Message}");
        }
    }

    public async Task<string?> GetLatestBackupContentAsync()
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Google Drive service not initialized.");
            
        try
        {
            return await _js.InvokeAsync<string?>("googleDriveInterop.getLatestBackup");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to get backup: {ex.Message}");
            throw;
        }
    }

    public async Task<DateTimeOffset?> SaveBackupAsync(string content)
    {
        if (!_isInitialized)
            throw new InvalidOperationException("Google Drive service not initialized.");
            
        try
        {
            var result = await _js.InvokeAsync<GoogleDriveFileMetadata>("googleDriveInterop.saveBackup", content);
            if (result != null && DateTimeOffset.TryParse(result.ModifiedTime, out var modifiedTime))
            {
                return modifiedTime;
            }
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to save backup: {ex.Message}");
            throw;
        }
    }

    private class GoogleDriveFileMetadata
    {
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? ModifiedTime { get; set; }
    }

    public async Task<DateTimeOffset?> GetBackupLastModifiedAsync()
    {
        if (!_isInitialized)
            return null;
            
        try
        {
            var isoString = await _js.InvokeAsync<string?>("googleDriveInterop.getBackupLastModified");
            if (!string.IsNullOrEmpty(isoString) && DateTimeOffset.TryParse(isoString, out var result))
            {
                return result;
            }
            return null;
        }
        catch
        {
            return null;
        }
    }
}
