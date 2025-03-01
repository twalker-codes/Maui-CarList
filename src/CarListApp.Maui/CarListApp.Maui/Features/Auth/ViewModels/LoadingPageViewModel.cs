using CarListApp.Maui.Core.Navigation;
using CarListApp.Maui.Core.Security;
using CarListApp.Maui.Features.Auth.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Serilog;

namespace CarListApp.Maui.Features.Auth.ViewModels
{
    public partial class LoadingPageViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly ITokenService _tokenService;

        public LoadingPageViewModel(
            IAuthService authService, 
            INavigationService navigationService,
            ITokenService tokenService)
        {
            _authService = authService;
            _navigationService = navigationService;
            _tokenService = tokenService;
            
            // Delay the check to ensure Shell is initialized
            MainThread.BeginInvokeOnMainThread(async () => 
            {
                try
                {
                    Log.Information("Starting authentication check");
                    // Add a small delay to ensure Shell is fully initialized
                    await Task.Delay(500);
                    await CheckAuthenticationAsync();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error in LoadingPageViewModel initialization");
                    await NavigateToLoginWithError();
                }
            });
        }

        private async Task CheckAuthenticationAsync()
        {
            try
            {
                Log.Debug("Checking for stored token");
                var token = await _tokenService.GetTokenAsync();
                
                if (string.IsNullOrEmpty(token))
                {
                    Log.Information("No token found, redirecting to login");
                    await _navigationService.NavigateToLoginAsync();
                    return;
                }

                Log.Debug("Validating stored token");
                if (!_tokenService.IsTokenValid(token))
                {
                    Log.Warning("Stored token is invalid or expired");
                    await _tokenService.RemoveTokenAsync();
                    await _navigationService.NavigateToLoginAsync();
                    return;
                }

                Log.Debug("Verifying authentication status with server");
                var isAuthenticated = await _authService.IsAuthenticatedAsync();
                Log.Information("Authentication check result: {IsAuthenticated}", isAuthenticated);
                
                if (!isAuthenticated)
                {
                    Log.Warning("Server rejected authentication");
                    await _tokenService.RemoveTokenAsync();
                    await _navigationService.NavigateToLoginAsync();
                    return;
                }

                // Double check token is still available after server check
                token = await _tokenService.GetTokenAsync();
                if (string.IsNullOrEmpty(token))
                {
                    Log.Warning("Token disappeared after server check");
                    await _navigationService.NavigateToLoginAsync();
                    return;
                }

                Log.Information("Authentication successful, proceeding to main page");
                await _navigationService.NavigateToMainAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Authentication check failed");
                await NavigateToLoginWithError();
            }
        }

        private async Task NavigateToLoginWithError()
        {
            try
            {
                await _tokenService.RemoveTokenAsync();
                await _navigationService.NavigateToLoginAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to navigate to login after error");
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        "An error occurred during authentication. Please try again.",
                        "OK");
                }
            }
        }
    }
} 