using CarListApp.Maui.Features.Auth.Services;
using CarListApp.Maui.Features.Shell.Interfaces;
using CarListApp.Maui.Features.Auth.Views;
using CarListApp.Maui.Features.Auth.ViewModels;
using CarListApp.Maui.Features.Auth.Models;
using Microsoft.Maui.Controls;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CarListApp.Maui.Features.Shell.Services;

public class ShellNavigationService : IShellNavigationService
{
    private readonly IShellConfigurationService _shellConfigurationService;
    private readonly IAuthService _authService;
    private readonly IServiceProvider _serviceProvider;

    public ShellNavigationService(
        IShellConfigurationService shellConfigurationService,
        IAuthService authService,
        IServiceProvider serviceProvider)
    {
        _shellConfigurationService = shellConfigurationService ?? throw new ArgumentNullException(nameof(shellConfigurationService));
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        Log.Information("ShellNavigationService initialized");
    }

    public async Task NavigateToLoginAsync()
    {
        try
        {
            Log.Debug("Starting navigation to login");
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    var shell = Microsoft.Maui.Controls.Shell.Current;
                    if (shell == null)
                    {
                        Log.Error("Shell.Current is null");
                        throw new InvalidOperationException("Shell.Current is null");
                    }

                    // Clear navigation stack and navigate to login
                    await shell.Navigation.PopToRootAsync(false);
                    await Task.Delay(50); // Small delay to ensure stack is cleared
                    await shell.GoToAsync("//login", false);
                    Log.Information("Successfully navigated to login page");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error during login navigation: {Message}", ex.Message);
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in NavigateToLoginAsync");
            throw;
        }
    }

    public async Task NavigateToMainAsync()
    {
        try
        {
            Log.Debug("Starting navigation to main");
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    var shell = Microsoft.Maui.Controls.Shell.Current;
                    if (shell == null)
                    {
                        Log.Error("Shell.Current is null");
                        throw new InvalidOperationException("Shell.Current is null");
                    }

                    // Verify authentication
                    if (!await _authService.IsAuthenticatedAsync())
                    {
                        Log.Warning("User is not authenticated, redirecting to login");
                        await NavigateToLoginAsync();
                        return;
                    }

                    var user = await _authService.GetCurrentUserAsync();
                    Log.Debug("Configuring shell for user role: {Role}", user.Role);
                    _shellConfigurationService.ConfigureShell(user.Role);

                    // Clear navigation stack and navigate to main
                    await shell.Navigation.PopToRootAsync(false);
                    await Task.Delay(50); // Small delay to ensure stack is cleared
                    await shell.GoToAsync("//main/cars/list", false);
                    Log.Information("Successfully navigated to main page");
                }
                catch (UnauthorizedAccessException uaEx)
                {
                    Log.Warning(uaEx, "Unauthorized access when navigating to main page");
                    await NavigateToLoginAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error during main navigation: {Message}", ex.Message);
                    await NavigateToLoginAsync();
                }
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in NavigateToMainAsync");
            throw;
        }
    }
} 