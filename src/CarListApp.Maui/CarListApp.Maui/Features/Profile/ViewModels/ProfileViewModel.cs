using CarListApp.Maui.Core.Navigation;
using CarListApp.Maui.Core.Theming.Models;
using CarListApp.Maui.Core.Theming.Services;
using CarListApp.Maui.Features.Auth.Services;
using CarListApp.Maui.Features.Profile.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace CarListApp.Maui.Features.Profile.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly ThemeSettingsService _themeService;

    [ObservableProperty]
    private UserInfo userInfo = new();

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public Theme CurrentTheme => ThemeSettingsService.Instance.Theme;

    public ProfileViewModel(
        IAuthService authService,
        INavigationService navigationService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _themeService = ThemeSettingsService.Instance;
        
        Log.Information("ProfileViewModel initialized");
    }

    [RelayCommand]
    private async Task LoadUserInfo()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var info = await _authService.GetCurrentUserAsync();
            if (info != null)
            {
                UserInfo = info;
                _themeService.RefreshTheme();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading user info");
            ErrorMessage = "Failed to load user information";
            await ShowErrorAlert("Failed to load user information");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await _authService.LogoutAsync();
            await _navigationService.NavigateToLoginAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during logout");
            ErrorMessage = "Failed to logout";
            await ShowErrorAlert("Failed to logout");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    private async Task ShowErrorAlert(string message)
    {
        if (Application.Current?.MainPage != null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", message, "OK");
        }
    }
}