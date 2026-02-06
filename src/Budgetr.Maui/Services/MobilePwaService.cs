using Budgetr.Shared.Services;

namespace Budgetr.Maui.Services;

public class MobilePwaService : IPwaService
{
    public bool IsInstallable => false; // Already installed as a native app

    public bool IsOnline => Connectivity.Current.NetworkAccess == NetworkAccess.Internet;

    public event Action? OnStateChanged;

    public MobilePwaService()
    {
        Connectivity.Current.ConnectivityChanged += OnConnectivityChanged;
    }

    private void OnConnectivityChanged(object? sender, ConnectivityChangedEventArgs e)
    {
        OnStateChanged?.Invoke();
    }

    public Task InstallAppAsync()
    {
        return Task.CompletedTask; // No-op
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask; // No-op
    }
}
