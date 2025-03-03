using CarListApp.Maui.Core.Navigation;
using CarListApp.Maui.Features.Auth.Models;
using CarListApp.Maui.Features.Auth.Services;
using CarListApp.Maui.Core.Security;
using CarListApp.Maui.Core.Theming.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace CarListApp.Maui.Features.Auth.ViewModels
{
    public partial class LoginViewModel : ObservableObject
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;
        private readonly ICredentialService _credentialService;
        private readonly ITokenService _tokenService;
        private readonly ThemeSettingsService _themeService;

        public LoginViewModel(
            IAuthService authService, 
            INavigationService navigationService,
            ICredentialService credentialService,
            ITokenService tokenService)
        {
            _authService = authService;
            _navigationService = navigationService;
            _credentialService = credentialService;
            _tokenService = tokenService;
            _themeService = ThemeSettingsService.Instance;
            
            Log.Information("LoginViewModel initialized with {AuthService}, {NavigationService}, {CredentialService}, {TokenService}",
                authService.GetType().Name,
                navigationService.GetType().Name,
                credentialService.GetType().Name,
                tokenService.GetType().Name);
            MainThread.BeginInvokeOnMainThread(async () => await LoadSavedCredentialsAsync());
        }

        [ObservableProperty]
        private string username = string.Empty;

        [ObservableProperty]
        private string password = string.Empty;

        [ObservableProperty]
        private bool rememberMe;

        [ObservableProperty]
        private bool isLoading;

        private async Task LoadSavedCredentialsAsync()
        {
            try
            {
                Log.Debug("Loading saved credentials");
                var (savedUsername, savedPassword) = await _credentialService.GetSavedCredentialsAsync();
                
                if (!string.IsNullOrEmpty(savedUsername) && !string.IsNullOrEmpty(savedPassword))
                {
                    Log.Debug("Found saved credentials for user: {Username}, PasswordLength: {PasswordLength}", 
                        savedUsername, savedPassword.Length);
                    Username = savedUsername;
                    Password = savedPassword;
                    RememberMe = true;
                }
                else
                {
                    Log.Debug("No saved credentials found");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading saved credentials");
                if (ex.InnerException != null)
                {
                    Log.Error(ex.InnerException, "Inner exception while loading credentials");
                }
            }
        }

        [RelayCommand]
        async Task Login()
        {
            bool loginSuccessful = false;
            
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
                Log.Warning("Login attempted with empty credentials");
                await DisplayLoginMessage("Username and password are required.");
                return;
            }

            try
            {
                Log.Information("Login attempt started for user: {Username}", Username);
                IsLoading = true;

                // Clear any existing token before login
                await _tokenService.RemoveTokenAsync();
                Log.Debug("Cleared existing token before login attempt");

                var loginModel = new LoginModel(Username, Password);
                AuthResponse? response = null;
                
                try
                {
                    response = await _authService.LoginAsync(loginModel);
                    Log.Debug("AuthService.LoginAsync completed successfully");
                }
                catch (UnauthorizedAccessException uaEx)
                {
                    Log.Warning(uaEx, "Login failed due to unauthorized access");
                    await DisplayLoginMessage("Invalid username or password.");
                    return;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Unexpected error during login request");
                    await DisplayLoginMessage("An error occurred during login. Please try again.");
                    return;
                }

                if (response == null || string.IsNullOrEmpty(response.Token))
                {
                    Log.Warning("Login failed - No token received from auth service");
                    await DisplayLoginMessage("Invalid username or password.");
                    return;
                }

                // Save and verify token
                try
                {
                    Log.Debug("Saving token to secure storage");
                    await _tokenService.SaveTokenAsync(response.Token);
                    
                    var savedToken = await _tokenService.GetTokenAsync();
                    if (string.IsNullOrEmpty(savedToken))
                    {
                        Log.Error("Token verification failed - No token found in secure storage after login");
                        await DisplayLoginMessage("Authentication error. Please try again.");
                        return;
                    }

                    if (!_tokenService.IsTokenValid(savedToken))
                    {
                        Log.Error("Token verification failed - Saved token is invalid");
                        await _tokenService.RemoveTokenAsync();
                        await DisplayLoginMessage("Authentication error. Please try again.");
                        return;
                    }
                    
                    Log.Debug("Token saved and verified successfully");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to save or verify token");
                    await DisplayLoginMessage("Failed to save authentication token. Please try again.");
                    return;
                }

                // Verify API access
                try
                {
                    Log.Debug("Verifying API access with token");
                    var isAuthenticated = await _authService.IsAuthenticatedAsync();
                    if (!isAuthenticated)
                    {
                        Log.Error("API access verification failed - Server rejected token");
                        await _tokenService.RemoveTokenAsync();
                        await DisplayLoginMessage("Authentication error. Please try again.");
                        return;
                    }
                    Log.Debug("API access verification successful");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to verify API access");
                    await _tokenService.RemoveTokenAsync();
                    await DisplayLoginMessage("Failed to verify API access. Please try again.");
                    return;
                }

                // Handle RememberMe
                try
                {
                    if (RememberMe)
                    {
                        Log.Debug("RememberMe is enabled, saving credentials");
                        await _credentialService.SaveCredentialsAsync(Username, Password);
                    }
                    else
                    {
                        Log.Debug("RememberMe is disabled, clearing saved credentials");
                        await _credentialService.ClearCredentialsAsync();
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to handle RememberMe setting - continuing with login");
                }

                // Load user's theme preferences
                try
                {
                    Log.Debug("Loading user's theme preferences");
                    _themeService.RefreshTheme();
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Failed to load theme preferences - continuing with login");
                }

                loginSuccessful = true;
                Log.Information("Login successful, navigating to main page");
                await _navigationService.NavigateToMainAsync();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Login process failed");
                if (ex.InnerException != null)
                {
                    Log.Error(ex.InnerException, "Inner exception during login");
                }
                await DisplayLoginMessage("An error occurred during login. Please try again.");
            }
            finally
            {
                IsLoading = false;
                if (!loginSuccessful)
                {
                    Log.Debug("Login was not successful, ensuring we stay on login page");
                    try
                    {
                        await _navigationService.NavigateToLoginAsync();
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Error ensuring we stay on login page");
                    }
                }
                Password = string.Empty; // Clear password for security
                Log.Debug("Login process completed - Loading state reset and password cleared");
            }
        }

        private async Task DisplayLoginMessage(string message)
        {
            try
            {
                Log.Debug("Displaying login message: {Message}", message);
                if (Application.Current?.MainPage != null)
                {
                    if (message.Contains("Invalid username or password"))
                    {
                        message = "Invalid username or password. Please try:\n\n" +
                                 "Admin User:\n" +
                                 "Username: admin@localhost.com\n" +
                                 "Password: P@ssword1\n\n" +
                                 "Regular User:\n" +
                                 "Username: user@localhost.com\n" +
                                 "Password: P@ssword1";
                    }
                    await Application.Current.MainPage.DisplayAlert("Login", message, "OK");
                    Password = string.Empty;
                    Log.Debug("Login message displayed and password cleared");
                }
                else
                {
                    Log.Warning("Cannot display login message - MainPage is null");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error displaying login message: {Message}", message);
                if (ex.InnerException != null)
                {
                    Log.Error(ex.InnerException, "Inner exception while displaying message");
                }
            }
        }
    }
} 