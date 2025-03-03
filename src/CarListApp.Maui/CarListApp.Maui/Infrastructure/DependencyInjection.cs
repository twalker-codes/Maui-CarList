using CarListApp.Maui.Core.Http;
using CarListApp.Maui.Core.Navigation;
using CarListApp.Maui.Core.Security;
using CarListApp.Maui.Core.Theming.Services;
using CarListApp.Maui.Features.Auth.Services;
using CarListApp.Maui.Features.Auth.ViewModels;
using CarListApp.Maui.Features.Auth.Views;
using CarListApp.Maui.Features.Cars.Services;
using CarListApp.Maui.Features.Cars.ViewModels;
using CarListApp.Maui.Features.Cars.Views;
using CarListApp.Maui.Features.Profile.ViewModels;
using CarListApp.Maui.Features.Profile.Views;
using CarListApp.Maui.Features.Shell.Services;
using CarListApp.Maui.Features.Shell.Interfaces;
using Serilog;

namespace CarListApp.Maui.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            try
            {
                Log.Debug("Configuring dependency injection");

                // Shell and Navigation Services
                services.AddSingleton<IShellConfigurationService, ShellConfigurationService>();
                services.AddSingleton<IShellNavigationService, ShellNavigationService>();
                services.AddSingleton<INavigationService, AppNavigator>();

                // Security Services
                services.AddSingleton<ITokenService, SecureStorageTokenService>();
                services.AddSingleton<ICredentialService, SecureStorageCredentialService>();
                services.AddSingleton<IHttpClientFactory, AuthenticatedHttpClientFactory>();

                // Theme Services
                services.AddSingleton<IThemeService, ThemeService>();

                // Auth Services and ViewModels
                services.AddSingleton<IAuthService, AuthService>();
                services.AddSingleton<LoadingPageViewModel>();
                services.AddTransient<LoginViewModel>();
                services.AddSingleton<LogoutViewModel>();
                services.AddSingleton<ProfileViewModel>();
                services.AddSingleton<LoadingPage>();
                services.AddTransient<LoginPage>();
                services.AddSingleton<LogoutPage>();
                services.AddSingleton<ProfilePage>();

                // Car Services and ViewModels
                services.AddSingleton<ICarService, CarService>();
                services.AddSingleton<CarListViewModel>();
                services.AddTransient<CarEditViewModel>();
                services.AddSingleton<CarListPage>();
                services.AddTransient<CarEditPage>();

                Log.Information("Dependency injection configured successfully");
                return services;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error configuring dependency injection");
                throw;
            }
        }

        public static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
        {
            // Existing registrations...
            
            builder.Services.AddSingleton<IThemeService, ThemeService>();
            
            return builder;
        }
    }
} 