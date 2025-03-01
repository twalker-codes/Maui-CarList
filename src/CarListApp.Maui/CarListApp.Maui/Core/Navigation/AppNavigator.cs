using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Controls;
using Serilog;

namespace CarListApp.Maui.Core.Navigation
{
    public class AppNavigator : INavigationService
    {
        public AppNavigator()
        {
            Log.Information("AppNavigator initialized");
        }

        public Task InitializeAsync()
        {
            Log.Debug("AppNavigator: Initializing");
            return Task.CompletedTask;
        }

        public async Task NavigateToAsync(string route, bool isAbsolute = false)
        {
            try
            {
                Log.Debug($"AppNavigator: Navigating to route: {route}, isAbsolute: {isAbsolute}");
                
                if (isAbsolute)
                {
                    // For absolute routes, ensure we have the correct number of slashes
                    route = route.TrimStart('/');
                    route = $"///{route}";
                }

                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var shell = Shell.Current;
                    if (shell == null)
                    {
                        Log.Error("Shell.Current is null");
                        throw new InvalidOperationException("Shell.Current is null");
                    }

                    // Clear navigation stack for absolute routes
                    if (isAbsolute)
                    {
                        await shell.Navigation.PopToRootAsync(false);
                        await Task.Delay(50); // Small delay to ensure stack is cleared
                    }

                    await shell.GoToAsync(route, false);
                });
                
                Log.Information("Navigation completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Navigation failed: {Message}", ex.Message);
                throw;
            }
        }

        public async Task NavigateBackAsync()
        {
            try
            {
                Log.Debug("AppNavigator: Navigating back");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var shell = Shell.Current;
                    if (shell == null)
                    {
                        Log.Error("Shell.Current is null");
                        throw new InvalidOperationException("Shell.Current is null");
                    }

                    await shell.Navigation.PopAsync(false);
                });
                Log.Information("Navigation back completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Navigation back failed: {Message}", ex.Message);
                throw;
            }
        }

        public Task NavigateToLoginAsync()
        {
            Log.Debug("AppNavigator: Navigating to login");
            return NavigateToAsync("login", true);
        }

        public Task NavigateToMainAsync()
        {
            Log.Debug("AppNavigator: Navigating to main");
            return NavigateToAsync("main/cars/list", true);
        }

        public async Task GoToAsync(string route)
        {
            try
            {
                Log.Debug($"AppNavigator: Going to route: {route}");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var shell = Shell.Current;
                    if (shell == null)
                    {
                        Log.Error("Shell.Current is null");
                        throw new InvalidOperationException("Shell.Current is null");
                    }

                    await shell.GoToAsync(route, false);
                });
                Log.Information("Navigation completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Navigation failed: {Message}", ex.Message);
                throw;
            }
        }

        public async Task GoToAsync(string route, IDictionary<string, object> parameters)
        {
            try
            {
                Log.Debug($"AppNavigator: Going to route: {route} with parameters");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    var shell = Shell.Current;
                    if (shell == null)
                    {
                        Log.Error("Shell.Current is null");
                        throw new InvalidOperationException("Shell.Current is null");
                    }

                    await shell.GoToAsync(route, parameters);
                });
                Log.Information("Navigation with parameters completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Navigation with parameters failed: {Message}", ex.Message);
                throw;
            }
        }
    }
} 