namespace Budgetr.Shared.Services;

/// <summary>
/// Interface for Google Drive operations.
/// Platform-specific implementations handle the actual API calls.
/// </summary>
public interface IGoogleDriveService
{
    /// <summary>
    /// Initializes the Google Drive client with the OAuth client ID.
    /// </summary>
    /// <param name="clientId">OAuth 2.0 client ID from Google Cloud Console.</param>
    Task InitializeAsync(string clientId);
    
    /// <summary>
    /// Checks if the user is currently signed in.
    /// </summary>
    Task<bool> IsSignedInAsync();
    
    /// <summary>
    /// Gets the signed-in user's email address.
    /// </summary>
    Task<string?> GetUserEmailAsync();
    
    /// <summary>
    /// Initiates the sign-in flow.
    /// </summary>
    /// <returns>True if sign-in was successful.</returns>
    Task<bool> SignInAsync();
    
    /// <summary>
    /// Signs out the current user.
    /// </summary>
    Task SignOutAsync();
    
    /// <summary>
    /// Gets the content of the latest backup file from Google Drive.
    /// </summary>
    /// <returns>JSON content of the backup, or null if no backup exists.</returns>
    Task<string?> GetLatestBackupContentAsync();
    
    /// <summary>
    /// Saves content to the backup file in Google Drive.
    /// Creates the file if it doesn't exist, updates it if it does.
    /// </summary>
    /// <param name="content">JSON content to save.</param>
    Task<DateTimeOffset?> SaveBackupAsync(string content);
    
    /// <summary>
    /// Gets the last modified time of the backup file.
    /// </summary>
    Task<DateTimeOffset?> GetBackupLastModifiedAsync();
}
