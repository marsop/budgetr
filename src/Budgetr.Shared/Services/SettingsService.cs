using System.Text.Json;

namespace Budgetr.Shared.Services;

/// <summary>
/// Implementation of settings service with local storage persistence.
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly IStorageService _storage;
    private const string StorageKey = "budgetr_settings";
    private const string DefaultLanguage = "en";

    private string _language = DefaultLanguage;

    public string Language
    {
        get => _language;
        set
        {
            if (_language != value)
            {
                _language = value;
                OnSettingsChanged?.Invoke();
                _ = SaveAsync();
            }
        }
    }

    public event Action? OnSettingsChanged;

    public SettingsService(IStorageService storage)
    {
        _storage = storage;
    }

    public async Task LoadAsync()
    {
        var json = await _storage.GetItemAsync(StorageKey);
        if (!string.IsNullOrEmpty(json))
        {
            try
            {
                var data = JsonSerializer.Deserialize<SettingsData>(json);
                if (data != null)
                {
                    _language = string.IsNullOrEmpty(data.Language) ? DefaultLanguage : data.Language;
                }
            }
            catch
            {
                // If deserialization fails, keep defaults
                _language = DefaultLanguage;
            }
        }
        OnSettingsChanged?.Invoke();
    }

    public async Task SaveAsync()
    {
        var data = new SettingsData { Language = _language };
        var json = JsonSerializer.Serialize(data);
        await _storage.SetItemAsync(StorageKey, json);
    }
}

/// <summary>
/// Data structure for settings persistence.
/// </summary>
internal class SettingsData
{
    public string Language { get; set; } = "en";
}
