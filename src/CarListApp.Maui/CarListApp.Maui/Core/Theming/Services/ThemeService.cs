using CarListApp.Maui.Core.Theming.Models;
using System.Text.Json;
using Serilog;

namespace CarListApp.Maui.Core.Theming.Services;

public class ThemeService : IThemeService
{
    private const string ThemeConfigKey = "theme_config_{0}";
    private ThemeConfig _currentTheme = new();
    private readonly ILogger _logger;
    public event EventHandler<ThemeConfig>? ThemeChanged;

    public bool IsDarkMode => _currentTheme.IsDarkMode;
    public string CurrentPrimaryColor => _currentTheme.PrimaryColor;

    public ThemeService(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _logger.Information("ThemeService initialized");
    }

    public async Task<ThemeConfig> GetThemeConfigAsync(string userId)
    {
        try
        {
            var key = string.Format(ThemeConfigKey, userId);
            string? storedConfig = null;

            try
            {
                storedConfig = await SecureStorage.Default.GetAsync(key);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to retrieve theme config from SecureStorage, falling back to Preferences");
                storedConfig = Preferences.Default.Get(key, string.Empty);
            }
            
            if (string.IsNullOrEmpty(storedConfig))
            {
                var defaultConfig = new ThemeConfig { UserId = userId };
                _currentTheme = defaultConfig;
                return defaultConfig;
            }

            var config = JsonSerializer.Deserialize<ThemeConfig>(storedConfig) 
                ?? new ThemeConfig { UserId = userId };
            _currentTheme = config;
            return config;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving theme config");
            var defaultConfig = new ThemeConfig { UserId = userId };
            _currentTheme = defaultConfig;
            return defaultConfig;
        }
    }

    public async Task SaveThemeConfigAsync(ThemeConfig config)
    {
        try
        {
            var key = string.Format(ThemeConfigKey, config.UserId);
            var json = JsonSerializer.Serialize(config);
            
            try
            {
                await SecureStorage.Default.SetAsync(key, json);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to save theme config to SecureStorage, falling back to Preferences");
                Preferences.Default.Set(key, json);
            }

            _currentTheme = config;
            ThemeChanged?.Invoke(this, config);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error saving theme config");
            throw;
        }
    }

    public async Task ApplyThemeAsync(ThemeConfig config)
    {
        if (Application.Current == null) return;

        try
        {
            var tcs = new TaskCompletionSource();
            
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                try
                {
                    // Set the app theme using MAUI's built-in mechanism
                    Application.Current.UserAppTheme = !config.IsDarkMode ? AppTheme.Dark : AppTheme.Light;

                    // Force immediate theme update
                    Application.Current.RequestedThemeChanged += OnThemeChanged;
                    void OnThemeChanged(object? sender, AppThemeChangedEventArgs e)
                    {
                        Application.Current.RequestedThemeChanged -= OnThemeChanged;
                        tcs.TrySetResult();
                    }

                    // Update primary color if it exists in resources
                    if (Application.Current.Resources.TryGetValue("Primary", out var _))
                    {
                        Application.Current.Resources["Primary"] = Color.FromArgb(config.PrimaryColor);
                    }

                    _logger.Information("Theme change initiated: IsDarkMode={IsDarkMode}, PrimaryColor={PrimaryColor}", 
                        config.IsDarkMode, config.PrimaryColor);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error applying theme on main thread");
                    tcs.TrySetException(ex);
                }
            });

            // Wait for theme change to complete with a timeout
            await Task.WhenAny(tcs.Task, Task.Delay(2000));

            await SaveThemeConfigAsync(config);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error applying theme");
            throw;
        }
    }

    public async Task SetDarkModeAsync(bool isDarkMode)
    {
        _currentTheme.IsDarkMode = isDarkMode;
        await ApplyThemeAsync(_currentTheme);
    }

    public async Task SetPrimaryColorAsync(string colorHex)
    {
        _currentTheme.PrimaryColor = colorHex;
        await ApplyThemeAsync(_currentTheme);
    }
} 