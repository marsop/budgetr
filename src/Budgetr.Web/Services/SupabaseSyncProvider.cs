using Budgetr.Shared.Services;

namespace Budgetr.Web.Services;

/// <summary>
/// Supabase implementation of ISyncProvider.
/// </summary>
public class SupabaseSyncProvider : ISyncProvider
{
    private readonly SupabaseService _supabaseService;
    
    public string Name => "Supabase";
    public string Description => "Sync your data with Supabase";
    public string Icon => "🗄️";
    
    public bool IsConfigured => !string.IsNullOrEmpty(_supabaseService.GetConfiguredUrl());

    public SupabaseSyncProvider(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        await _supabaseService.TryAutoInitializeAsync();
        return await _supabaseService.IsSignedInAsync();
    }

    public async Task<bool> AuthenticateAsync()
    {
        // Authentication for Supabase requires email/password,
        // which is handled directly by the Settings UI.
        // This method checks if already authenticated.
        return await _supabaseService.IsSignedInAsync();
    }

    public async Task SignOutAsync()
    {
        await _supabaseService.SignOutAsync();
    }

    public async Task<string?> DownloadDataAsync()
    {
        return await _supabaseService.GetLatestBackupContentAsync();
    }

    public async Task UploadDataAsync(string jsonData)
    {
        await _supabaseService.SaveBackupAsync(jsonData);
    }

    public async Task<DateTimeOffset?> GetLastBackupTimeAsync()
    {
        return await _supabaseService.GetBackupLastModifiedAsync();
    }
}
