using CarListApp.Maui.Features.Auth.Views;
using CarListApp.Maui.Features.Cars.Views;
using CarListApp.Maui.Features.Auth.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Controls;
using Serilog;

namespace CarListApp.Maui;

public partial class AppShell : Shell
{
    private readonly IServiceProvider _serviceProvider;

    public AppShell(IServiceProvider serviceProvider)
    {
        try
        {
            Log.Information("Initializing AppShell");
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            InitializeComponent();
            RegisterRoutes();
            ConfigureShellAppearance();
            Log.Information("AppShell initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error initializing AppShell");
            throw;
        }
    }

    protected override async void OnNavigating(ShellNavigatingEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);
        
        try
        {
            base.OnNavigating(args);
            
            var targetLocation = args.Target?.Location?.OriginalString;
            Log.Debug("Shell navigating to {Target}", targetLocation);

            if (args.Target?.Location == null)
            {
                Log.Warning("Navigation target is null");
                return;
            }

            // Ensure we're on the main thread
            if (!MainThread.IsMainThread)
            {
                Log.Debug("Navigation not on main thread, dispatching");
                args.Cancel();
                await MainThread.InvokeOnMainThreadAsync(() => 
                    GoToAsync(args.Target.Location.OriginalString));
                return;
            }

            // Handle navigation based on authentication state
            if (targetLocation?.Contains("main", StringComparison.OrdinalIgnoreCase) == true && 
                !await IsAuthenticated())
            {
                Log.Warning("Unauthorized navigation attempt to main section");
                args.Cancel();
                await GoToAsync("///login");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in OnNavigating");
            args.Cancel();
            try
            {
                await GoToAsync("///login");
            }
            catch (Exception navEx)
            {
                Log.Error(navEx, "Failed to navigate to login after error");
            }
        }
    }

    protected override void OnNavigated(ShellNavigatedEventArgs args)
    {
        ArgumentNullException.ThrowIfNull(args);
        
        try
        {
            base.OnNavigated(args);
            Log.Information("Shell navigated to {Current}", args.Current?.Location?.OriginalString);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in OnNavigated");
        }
    }

    private void RegisterRoutes()
    {
        try
        {
            Log.Debug("Registering shell routes");

            // Register routes for navigation
            Routing.RegisterRoute("car/edit", typeof(CarEditPage));
            Routing.RegisterRoute("car/details", typeof(CarEditPage));
            Routing.RegisterRoute("profile/details", typeof(ProfilePage));
            Routing.RegisterRoute("profile/edit", typeof(ProfilePage));

            Log.Information("Shell routes registered successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error registering routes");
            throw;
        }
    }

    private void ConfigureShellAppearance()
    {
        try
        {
            Log.Debug("Configuring shell appearance");

            // Configure shell appearance
            Shell.SetNavBarIsVisible(this, false);
            Shell.SetFlyoutBehavior(this, FlyoutBehavior.Disabled);

            // Configure tab bar appearance
            Shell.SetTabBarBackgroundColor(this, Colors.White);
            Shell.SetTabBarUnselectedColor(this, Color.FromArgb("#666666"));
            Shell.SetTabBarTitleColor(this, Color.FromArgb("#6B4EFF"));

            Log.Information("Shell appearance configured successfully");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error configuring shell appearance");
            throw;
        }
    }

    private async Task<bool> IsAuthenticated()
    {
        try
        {
            var authService = _serviceProvider.GetRequiredService<IAuthService>();
            return await authService.IsAuthenticatedAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error checking authentication status");
            return false;
        }
    }
}