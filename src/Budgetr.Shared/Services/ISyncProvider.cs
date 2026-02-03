namespace Budgetr.Shared.Services;

/// <summary>
/// Interface for cloud synchronization providers.
/// Allows different storage backends (Google Drive, OneDrive, Dropbox, etc.)
/// to be used for data backup and restore.
/// </summary>
public interface ISyncProvider
{
    /// <summary>
    /// Display name of the provider (e.g., "Google Drive").
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Short description of the provider.
    /// </summary>
    string Description { get; }
    
    /// <summary>
    /// Icon/emoji for the provider.
    /// </summary>
    string Icon { get; }
    
    /// <summary>
    /// Whether the provider has been configured with necessary credentials.
    /// </summary>
    bool IsConfigured { get; }
    
    /// <summary>
    /// Whether the user is currently authenticated with the provider.
    /// </summary>
    Task<bool> IsAuthenticatedAsync();
    
    /// <summary>
    /// Initiates the authentication flow.
    /// </summary>
    /// <returns>True if authentication was successful.</returns>
    Task<bool> AuthenticateAsync();
    
    /// <summary>
    /// Signs out from the provider.
    /// </summary>
    Task SignOutAsync();
    
    /// <summary>
    /// Downloads the latest backup data from the provider.
    /// </summary>
    /// <returns>JSON string of the backup data, or null if no backup exists.</returns>
    Task<string?> DownloadDataAsync();
    
    /// <summary>
    /// Uploads data to the provider as a backup.
    /// </summary>
    /// <param name="jsonData">JSON string to backup.</param>
    Task UploadDataAsync(string jsonData);
    
    /// <summary>
    /// Gets the timestamp of the last backup, if available.
    /// </summary>
    Task<DateTimeOffset?> GetLastBackupTimeAsync();
}
