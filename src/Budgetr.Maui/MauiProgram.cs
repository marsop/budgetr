using Budgetr.Maui.Services;
using Budgetr.Shared.Services;
using Microsoft.Extensions.Logging;

namespace Budgetr.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
			});

		builder.Services.AddMauiBlazorWebView();

        // Platform specific services
        builder.Services.AddSingleton<IPwaService, MobilePwaService>();
        builder.Services.AddScoped<IStorageService, MauiStorageService>();

        // Shared services
        builder.Services.AddLocalization();
        builder.Services.AddScoped<IMeterConfigurationService, MeterConfigurationService>();
        builder.Services.AddScoped<ISettingsService, SettingsService>();
        builder.Services.AddScoped<ITimeTrackingService, TimeTrackingService>();
        builder.Services.AddScoped<ITutorialService, TutorialService>();
        
        // Optional/Other
        builder.Services.AddScoped<GoogleDriveService>();
        builder.Services.AddScoped<IAutoSyncService, AutoSyncService>();

#if DEBUG
		builder.Services.AddBlazorWebViewDeveloperTools();
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
