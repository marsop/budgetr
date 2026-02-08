using Budgetr.Shared.Services;

namespace Budgetr.Maui.Services;

public class MauiStorageService : IStorageService
{
    public Task<string?> GetItemAsync(string key)
    {
        var value = Preferences.Default.Get(key, string.Empty);
        return Task.FromResult(string.IsNullOrEmpty(value) ? null : value);
    }

    public Task SetItemAsync(string key, string value)
    {
        Preferences.Default.Set(key, value);
        return Task.CompletedTask;
    }

    public Task RemoveItemAsync(string key)
    {
        Preferences.Default.Remove(key);
        return Task.CompletedTask;
    }
}
