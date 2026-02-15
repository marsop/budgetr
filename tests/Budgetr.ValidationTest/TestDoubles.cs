using Budgetr.Shared.Models;
using Budgetr.Shared.Services;

namespace Budgetr.ValidationTest;

internal sealed class InMemoryStorageService : IStorageService
{
    private readonly Dictionary<string, string> _store = new();

    public Task<string?> GetItemAsync(string key)
    {
        _store.TryGetValue(key, out var value);
        return Task.FromResult<string?>(value);
    }

    public Task SetItemAsync(string key, string value)
    {
        _store[key] = value;
        return Task.CompletedTask;
    }

    public Task RemoveItemAsync(string key)
    {
        _store.Remove(key);
        return Task.CompletedTask;
    }
}

internal sealed class StubMeterConfigurationService : IMeterConfigurationService
{
    private readonly List<Meter> _meters;

    public int LoadCalls { get; private set; }

    public StubMeterConfigurationService(IEnumerable<Meter> meters)
    {
        _meters = meters.ToList();
    }

    public Task<List<Meter>> LoadMetersAsync()
    {
        LoadCalls++;
        return Task.FromResult(_meters.Select(CloneMeter).ToList());
    }

    private static Meter CloneMeter(Meter meter)
    {
        return new Meter
        {
            Id = meter.Id,
            Name = meter.Name,
            Factor = meter.Factor,
            DisplayOrder = meter.DisplayOrder
        };
    }
}

internal sealed class StubSettingsService : ISettingsService
{
    public string Language { get; set; } = "en";
    public bool TutorialCompleted { get; set; }
    public event Action? OnSettingsChanged;

    public Task LoadAsync() => Task.CompletedTask;
    public Task SaveAsync() => Task.CompletedTask;

    public Task SetLanguageAsync(string language)
    {
        Language = language;
        OnSettingsChanged?.Invoke();
        return Task.CompletedTask;
    }
}
