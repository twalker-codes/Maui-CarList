using Microsoft.Maui.ApplicationModel;

public interface IThemeService
{
    void SetTheme(AppTheme theme);
    bool IsDarkMode { get; }
}

public class ThemeService : IThemeService
{
    public bool IsDarkMode => Application.Current?.UserAppTheme == AppTheme.Dark;

    public void SetTheme(AppTheme theme)
    {
        if (Application.Current != null)
        {
            // Set the theme
            Application.Current.UserAppTheme = theme;

            // Force theme refresh by temporarily switching to system theme and back
            var currentTheme = Application.Current.UserAppTheme;
            Application.Current.UserAppTheme = AppTheme.Unspecified;
            MainThread.BeginInvokeOnMainThread(() => 
            {
                Application.Current.UserAppTheme = currentTheme;
            });
        }
    }
} 