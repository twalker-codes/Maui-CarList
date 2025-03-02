using CarListApp.Maui.Features.Shell.Interfaces;
using Microsoft.Maui.Controls;
using Serilog;

namespace CarListApp.Maui.Core.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly IShellNavigationService _shellNavigationService;

        public NavigationService(IShellNavigationService shellNavigationService)
        {
            _shellNavigationService = shellNavigationService;
            Log.Information("NavigationService initialized");
        }

        public async Task NavigateToAsync(string route, bool isAbsolute = false)
        {
            try
            {
                Log.Debug($"NavigationService: Navigating to route: {route}, isAbsolute: {isAbsolute}");
                var finalRoute = isAbsolute ? $"///{route}" : route;
                await Shell.Current.GoToAsync(finalRoute);
                Log.Information("Navigation completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Navigation failed");
                throw;
            }
        }

        public async Task NavigateBackAsync()
        {
            try
            {
                Log.Debug("NavigationService: Navigating back");
                await Shell.Current.GoToAsync("..");
                Log.Information("Navigation back completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Navigation back failed");
                throw;
            }
        }

        public Task InitializeAsync()
        {
            Log.Debug("NavigationService: Initializing");
            return Task.CompletedTask;
        }

        public Task NavigateToLoginAsync()
        {
            Log.Debug("NavigationService: Navigating to login");
            return _shellNavigationService.NavigateToLoginAsync();
        }

        public Task NavigateToMainAsync()
        {
            Log.Debug("NavigationService: Navigating to main");
            return _shellNavigationService.NavigateToMainAsync();
        }

        public async Task GoToAsync(string route)
        {
            try
            {
                Log.Debug($"NavigationService: Going to route: {route}");
                await Shell.Current.GoToAsync(route);
                Log.Information("Navigation completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Navigation failed");
                throw;
            }
        }

        public async Task GoToAsync(string route, IDictionary<string, object> parameters)
        {
            try
            {
                Log.Debug($"NavigationService: Going to route: {route} with parameters");
                await Shell.Current.GoToAsync(route, parameters);
                Log.Information("Navigation with parameters completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Navigation with parameters failed");
                throw;
            }
        }
    }
} 