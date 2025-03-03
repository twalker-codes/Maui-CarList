using CarListApp.Maui.Core.Theming.Models;

namespace CarListApp.Maui.Core.Theming.Services;

public interface IThemeService
{
    event EventHandler<ThemeConfig> ThemeChanged;
    Task<ThemeConfig> GetThemeConfigAsync(string userId);
    Task SetPrimaryColorAsync(string colorHex);
    void SetTheme(AppTheme theme);
}