using Budgetr.Shared.Models;
using Budgetr.Shared.Services;
using System.Text.Json;

try
{
    Console.WriteLine("Testing AddMeter Functionality...");

    var storage = new MockStorageService();
    var config = new MockMeterConfigService();
    var settings = new MockSettingsService();
    var service = new TimeTrackingService(storage, config, settings);
    
    // Initialize (LoadAsync)
    await service.LoadAsync();
    Console.WriteLine($"Initial meters count: {service.Account.Meters.Count}"); // Should be 2 from mock

    // Test 1: Add Valid Meter
    try
    {
        service.AddMeter("New Meter", 2.0);
        Console.WriteLine("Test 1 (Add Valid): PASS");
        if (service.Account.Meters.Count != 3) Console.WriteLine("  ERROR: Meter count incorrect");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Test 1 (Add Valid): FAIL - {ex.Message}");
    }

    // Test 2: Add Duplicate Factor
    try
    {
        service.AddMeter("Duplicate", 2.0);
        Console.WriteLine("Test 2 (Add Duplicate Factor): FAIL (Expected ArgumentException)");
    }
    catch (ArgumentException)
    {
        Console.WriteLine("Test 2 (Add Duplicate Factor): PASS");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Test 2 (Add Duplicate Factor): FAIL - Wrong exception {ex.GetType().Name}");
    }

    // Test 3: Add Invalid Name (Empty)
    try
    {
        service.AddMeter("", 3.0);
        Console.WriteLine("Test 3 (Invalid Name): FAIL (Expected ArgumentException)");
    }
    catch (ArgumentException)
    {
        Console.WriteLine("Test 3 (Invalid Name): PASS");
    }

    // Test 4: Add Invalid Factor Range
    try
    {
        service.AddMeter("Invalid Range", 11.0);
        Console.WriteLine("Test 4 (Invalid Range): FAIL (Expected ArgumentOutOfRangeException)");
    }
    catch (ArgumentOutOfRangeException)
    {
        Console.WriteLine("Test 4 (Invalid Range): PASS");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Test 4 (Invalid Range): FAIL - Wrong exception {ex.GetType().Name} - {ex.Message}");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Unhandled exception: {ex}");
}

// Mocks
class MockStorageService : IStorageService
{
    private Dictionary<string, string> _store = new();
    
    public Task<string?> GetItemAsync(string key)
    {
        _store.TryGetValue(key, out var val);
        return Task.FromResult(val);
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

class MockMeterConfigService : IMeterConfigurationService
{
    public Task<List<Meter>> LoadMetersAsync()
    {
        return Task.FromResult(new List<Meter>
        {
            new Meter { Name = "M1", Factor = 1.0 },
            new Meter { Name = "M2", Factor = -1.0 }
        });
    }
}

class MockSettingsService : ISettingsService
{
    public string Language { get; set; } = "en";
    public bool TutorialCompleted { get; set; }
    public event Action? OnSettingsChanged;

    public Task LoadAsync() => Task.CompletedTask;
    public Task SaveAsync() => Task.CompletedTask;
    public Task SetLanguageAsync(string language) 
    {
        Language = language;
        return Task.CompletedTask;
    }
}
