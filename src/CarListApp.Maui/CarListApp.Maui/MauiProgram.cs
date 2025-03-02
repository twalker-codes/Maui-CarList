using CarListApp.Maui.Features.Auth.ViewModels;
using CarListApp.Maui.Features.Auth.Views;
using CarListApp.Maui.Features.Cars.Services;
using CarListApp.Maui.Features.Cars.ViewModels;
using CarListApp.Maui.Features.Cars.Views;
using CarListApp.Maui.Infrastructure;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using CommunityToolkit.Maui;
using CarListApp.Maui.Core.Theming.Services;
using ILogger = Serilog.ILogger;

namespace CarListApp.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .WriteTo.Debug()
            .WriteTo.Console()
            .WriteTo.File(Path.Combine(FileSystem.AppDataDirectory, "logs/carlist-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7)
            .CreateLogger();

        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSans");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                fonts.AddFont("fa-regular-400.ttf", "FontAwesomeRegular");
                fonts.AddFont("fa-solid-900.ttf", "FontAwesomeSolid");
            });

#if DEBUG
        builder.Logging.AddDebug();
        builder.Logging.AddSerilog(Log.Logger);
#endif

        // Register Serilog Logger
        builder.Services.AddSingleton<ILogger>(Log.Logger);

        // Add infrastructure services
        builder.Services.AddInfrastructure();

        // Register database
        string dbPath = Path.Combine(FileSystem.AppDataDirectory, "cars.db3");
        builder.Services.AddSingleton(s => ActivatorUtilities.CreateInstance<CarDatabaseService>(s, dbPath));

        // Register Auth Feature
        builder.Services.AddSingleton<LoadingPageViewModel>();
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<LogoutViewModel>();
        builder.Services.AddSingleton<LoadingPage>();
        builder.Services.AddSingleton<LoginPage>();

        // Register Car Feature
        builder.Services.AddSingleton<ICarService, CarService>();
        builder.Services.AddSingleton<CarListViewModel>();
        builder.Services.AddTransient<CarEditViewModel>();
        builder.Services.AddSingleton<CarListPage>();
        builder.Services.AddTransient<CarEditPage>();

        // Register Theme Service
        builder.Services.AddSingleton<IPreferences>(Preferences.Default);
        builder.Services.AddSingleton<IThemeService, ThemeService>();

        return builder.Build();
    }
}