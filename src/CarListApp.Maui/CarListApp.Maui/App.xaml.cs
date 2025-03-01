using CarListApp.Maui.Features.Auth.Models;
using CarListApp.Maui.Features.Cars.Services;
using Microsoft.Extensions.DependencyInjection;
using CarListApp.Maui.Infrastructure;
using Microsoft.Maui.Devices;
using Serilog;

namespace CarListApp.Maui;

public partial class App : Application
{
    public static UserInfo? UserInfo { get; set; }
    public static CarDatabaseService CarDatabaseService { get; private set; } = null!;
    private readonly IServiceProvider _serviceProvider;
    private readonly CarDatabaseService _carDatabaseService;

    public App(IServiceProvider serviceProvider, CarDatabaseService carDatabaseService)
    {
        try
        {
            Log.Information("Initializing App");
            ArgumentNullException.ThrowIfNull(carDatabaseService);
            
            _serviceProvider = serviceProvider;
            _carDatabaseService = carDatabaseService;
            CarDatabaseService = carDatabaseService;
            
            InitializeComponent();
            MainPage = new AppShell(_serviceProvider);

            // Handle initial navigation after shell is created
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    // Ensure Shell is ready before navigation
                    await Task.Delay(100);
                    
                    if (Shell.Current is not Shell shell)
                    {
                        Log.Error("Shell.Current is null during initial navigation");
                        return;
                    }

                    // Navigate to loading page using absolute route
                    await shell.GoToAsync("///loading");
                    Log.Information("Initial navigation completed");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error during initial navigation");
                    try
                    {
                        if (Shell.Current is Shell fallbackShell)
                        {
                            await fallbackShell.GoToAsync("///login");
                        }
                    }
                    catch (Exception navEx)
                    {
                        Log.Error(navEx, "Failed to navigate to login after error");
                    }
                }
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error initializing App");
            throw;
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            var window = base.CreateWindow(activationState);

            // Set window size for desktop platforms
            if (DeviceInfo.Current.Platform == DevicePlatform.MacCatalyst ||
                DeviceInfo.Current.Platform == DevicePlatform.WinUI)
            {
                var displayInfo = DeviceDisplay.Current.MainDisplayInfo;
                window.Width = 1200;
                window.Height = 800;
                window.X = (displayInfo.Width / displayInfo.Density - window.Width) / 2;
                window.Y = (displayInfo.Height / displayInfo.Density - window.Height) / 2;
            }

            return window;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error creating window");
            throw;
        }
    }
}