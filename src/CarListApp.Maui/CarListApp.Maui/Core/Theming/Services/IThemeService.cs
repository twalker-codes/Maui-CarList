using CarListApp.Maui.Core.Theming.Models;

namespace CarListApp.Maui.Core.Theming.Services;

public interface IThemeService
{
    Task<ThemeConfig> GetThemeConfigAsync(string userId);
    Task SaveThemeConfigAsync(ThemeConfig config);
    Task ApplyThemeAsync(ThemeConfig config);
    Task SetDarkModeAsync(bool isDarkMode);
    Task SetPrimaryColorAsync(string colorHex);
    bool IsDarkMode { get; }
    string CurrentPrimaryColor { get; }
    event EventHandler<ThemeConfig> ThemeChanged;
} 