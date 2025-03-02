using System.ComponentModel;
using CarListApp.Maui.Core.Theming.Models;
using Microsoft.Maui.ApplicationModel;

namespace CarListApp.Maui.Core.Theming.Services;

public class ThemeSettingsService : INotifyPropertyChanged
{
    private const string ThemePreferenceKeyFormat = "AppTheme_{0}"; // {0} will be replaced with userId
    private static ThemeSettingsService? _instance;
    public static ThemeSettingsService Instance => _instance ??= new ThemeSettingsService();

    private Theme _theme = Theme.System; // Initialize with System theme
    public Theme Theme
    {
        get => _theme;
        set
        {
            if (_theme == value) return;
            _theme = value;
            OnPropertyChanged(nameof(Theme));
            SaveThemePreference();
            ApplyTheme();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private ThemeSettingsService()
    {
        LoadSavedTheme();
    }

    private string GetThemePreferenceKey()
    {
        var userId = App.UserInfo?.UserId ?? "default";
        return string.Format(ThemePreferenceKeyFormat, userId);
    }

    private void LoadSavedTheme()
    {
        var key = GetThemePreferenceKey();
        if (Preferences.ContainsKey(key))
        {
            var savedTheme = Preferences.Get(key, "System");
            Theme = Theme.AvailableThemes.FirstOrDefault(t => t.DisplayName == savedTheme) ?? Theme.System;
        }
        else
        {
            Theme = Theme.System;
        }
    }

    private void SaveThemePreference()
    {
        var key = GetThemePreferenceKey();
        Preferences.Set(key, Theme.DisplayName);
    }

    private void ApplyTheme()
    {
        if (Application.Current != null)
        {
            Application.Current.UserAppTheme = Theme.AppTheme;
        }
    }

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void RefreshTheme()
    {
        LoadSavedTheme();
    }
} 