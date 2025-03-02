using CarListApp.Maui.Core.Navigation;
using CarListApp.Maui.Features.Auth.Services;
using CarListApp.Maui.Core.Security;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Serilog;

namespace CarListApp.Maui.Features.Auth.ViewModels
{
    public partial class LogoutViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly ICredentialService _credentialService;

        public LogoutViewModel(
            IAuthService authService, 
            INavigationService navigationService,
            ICredentialService credentialService)
        {
            _authService = authService;
            _navigationService = navigationService;
            _credentialService = credentialService;
            Log.Information("LogoutViewModel initialized");
            MainThread.BeginInvokeOnMainThread(async () => await LogoutAsync());
        }

        private async Task LogoutAsync()
        {
            try
            {
                Log.Information("Starting logout process");

                // Clear saved credentials first
                Log.Debug("Clearing saved credentials");
                await _credentialService.ClearCredentialsAsync();

                // Clear authentication state
                Log.Debug("Clearing authentication state");
                await _authService.LogoutAsync();

                // Navigate to login page
                Log.Debug("Navigating to login page");
                await _navigationService.NavigateToLoginAsync();
                
                Log.Information("Logout process completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error during logout process");
                
                try
                {
                    // Even if logout fails, attempt to navigate to login
                    Log.Debug("Attempting to navigate to login page after error");
                    await _navigationService.NavigateToLoginAsync();
                }
                catch (Exception navEx)
                {
                    Log.Error(navEx, "Failed to navigate to login page after logout error");
                    if (Application.Current?.MainPage != null)
                    {
                        await Application.Current.MainPage.DisplayAlert(
                            "Error",
                            "An error occurred during logout. Please restart the application.",
                            "OK");
                    }
                }
            }
        }
    }
} 