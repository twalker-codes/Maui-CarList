using CarListApp.Maui.Features.Auth.Models;
using CarListApp.Maui.Features.Auth.Views;
using CarListApp.Maui.Features.Cars.Views;
using CarListApp.Maui.Features.Shell.Interfaces;
using CarListApp.Maui.Features.Cars.ViewModels;
using CarListApp.Maui.Features.Auth.ViewModels;
using CarListApp.Maui.Core.Constants;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CarListApp.Maui.Features.Shell.Services;

public class ShellConfigurationService : IShellConfigurationService
{
    private readonly IServiceProvider _serviceProvider;

    public ShellConfigurationService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        Log.Information("ShellConfigurationService initialized");
    }

    public void ConfigureShell(string userRole)
    {
        try
        {
            Log.Information("Starting shell configuration for user role: {Role}", userRole);

            if (!MainThread.IsMainThread)
            {
                Log.Debug("Switching to main thread for shell configuration");
                MainThread.BeginInvokeOnMainThread(() => ConfigureShell(userRole));
                return;
            }

            var shell = Microsoft.Maui.Controls.Shell.Current;
            if (shell == null)
            {
                var error = "Shell.Current is null during configuration";
                Log.Error(error);
                throw new InvalidOperationException(error);
            }

            try
            {
                // Set shell styles
                Log.Debug("Setting shell styles");
                shell.FlyoutBehavior = FlyoutBehavior.Disabled;
                shell.BackgroundColor = Colors.White;

                // Create tabs based on user role
                Log.Debug("Creating tabs for user role: {Role}", userRole);
                var tabs = new TabBar { 
                    Route = "main"
                };
                Microsoft.Maui.Controls.Shell.SetTabBarBackgroundColor(tabs, Colors.White);

                // Cars tab (available to all users)
                Log.Debug("Adding cars tab");
                var carListViewModel = _serviceProvider.GetRequiredService<CarListViewModel>();
                var carListPage = new CarListPage(carListViewModel)
                {
                    BackgroundColor = Colors.White
                };
                tabs.Items.Add(CreateTab("Cars", "cars", carListPage, "\uf1b9")); // Car icon

                // Admin-specific tabs
                if (userRole.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
                {
                    Log.Debug("Adding administrator-specific tabs");
                    var profileViewModel = _serviceProvider.GetRequiredService<ProfileViewModel>();
                    var profilePage = new ProfilePage(profileViewModel);
                    tabs.Items.Add(CreateTab("Users", "users", profilePage, "\uf007")); // User icon
                    
                    var settingsViewModel = _serviceProvider.GetRequiredService<ProfileViewModel>();
                    var settingsPage = new ProfilePage(settingsViewModel);
                    tabs.Items.Add(CreateTab("Settings", "settings", settingsPage, "\uf013")); // Cog icon
                }

                // Logout tab (available to all users)
                Log.Debug("Adding logout tab");
                var logoutViewModel = _serviceProvider.GetRequiredService<LogoutViewModel>();
                var logoutPage = new LogoutPage(logoutViewModel);
                tabs.Items.Add(CreateTab("Logout", "logout", logoutPage, "\uf2f5")); // Sign-out icon

                // Add tabs to shell
                Log.Debug("Adding tabs to shell");
                shell.Items.Clear();
                shell.Items.Add(tabs);

                Log.Information("Shell configuration completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during shell configuration setup");
                throw;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to configure shell");
            throw;
        }
    }

    private ShellContent CreateTab(string title, string route, Page page, string icon)
    {
        try
        {
            Log.Debug("Creating tab: {Title} with route: {Route}", title, route);
            var content = new ShellContent
            {
                Title = title,
                Route = route,
                Content = page,
                Icon = new FontImageSource
                {
                    FontFamily = "FontAwesomeSolid",
                    Glyph = icon,
                    Size = 22,
                    Color = Colors.Black
                }
            };
            Microsoft.Maui.Controls.Shell.SetBackgroundColor(content, Colors.White);

            return content;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to create tab: {Title}", title);
            throw;
        }
    }
} 