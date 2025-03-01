using CarListApp.Maui.Core.Navigation;
using CarListApp.Maui.Features.Auth.Services;
using CarListApp.Maui.Core.Theming.Services;
using CarListApp.Maui.Core.Theming.Models;
using CarListApp.Maui.Features.Auth.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Serilog;

namespace CarListApp.Maui.Features.Auth.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly INavigationService _navigationService;
    private readonly IThemeService _themeService;

    [ObservableProperty]
    private UserInfo userInfo = new();

    [ObservableProperty]
    private bool isDarkMode;

    [ObservableProperty]
    private string primaryColor = "#6B4EFF";

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    public ProfileViewModel(
        IAuthService authService,
        INavigationService navigationService,
        IThemeService themeService)
    {
        _authService = authService ?? throw new ArgumentNullException(nameof(authService));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        
        IsDarkMode = _themeService.IsDarkMode;
        PrimaryColor = _themeService.CurrentPrimaryColor;
        _themeService.ThemeChanged += OnThemeChanged;
        
        Log.Information("ProfileViewModel initialized");
    }

    private void OnThemeChanged(object? sender, ThemeConfig e)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsDarkMode = e.IsDarkMode;
            PrimaryColor = e.PrimaryColor;
        });
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
                var config = await _themeService.GetThemeConfigAsync(info.Username);
                if (config != null)
                {
                    IsDarkMode = config.IsDarkMode;
                    PrimaryColor = config.PrimaryColor;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading user info");
            ErrorMessage = "Failed to load user information";
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Failed to load user information",
                    "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleDarkMode()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                try
                {
                    await _themeService.SetDarkModeAsync(!IsDarkMode);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error toggling dark mode on main thread");
                    ErrorMessage = "Failed to toggle dark mode";
                }
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in ToggleDarkMode command");
            ErrorMessage = "Failed to toggle dark mode";
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Failed to toggle dark mode",
                    "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task UpdatePrimaryColor()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            await _themeService.SetPrimaryColorAsync(PrimaryColor);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating primary color");
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Failed to update primary color",
                    "OK");
            }
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
            if (Application.Current?.MainPage != null)
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "Failed to logout",
                    "OK");
            }
        }
        finally
        {
            IsBusy = false;
        }
    }
}