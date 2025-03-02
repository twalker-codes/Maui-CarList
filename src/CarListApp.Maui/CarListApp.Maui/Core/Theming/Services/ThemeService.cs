using CarListApp.Maui.Core.Theming.Models;
using System.Text.Json;
using Serilog;
using System.Text.RegularExpressions;

namespace CarListApp.Maui.Core.Theming.Services;

public class ThemeService : IThemeService
{
    private readonly IPreferences _preferences;
    private readonly ILogger _logger;
    private ThemeConfig _currentConfig;

    private const string DefaultPrimaryColor = "#6B4EFF";

    public bool IsDarkMode => _currentConfig.IsDarkMode;
    public string CurrentPrimaryColor => _currentConfig.PrimaryColor;

    public event EventHandler<ThemeConfig> ThemeChanged = delegate { };

    public ThemeService(IPreferences preferences, ILogger logger)
    {
        _preferences = preferences ?? throw new ArgumentNullException(nameof(preferences));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _currentConfig = new ThemeConfig();
    }

    public Task<ThemeConfig> GetThemeConfigAsync(string userId)
    {
        ArgumentNullException.ThrowIfNull(userId);

        try
        {
            var jsonConfig = _preferences.Get($"theme_config_{userId}", string.Empty);
            if (string.IsNullOrEmpty(jsonConfig))
                return Task.FromResult(new ThemeConfig { UserId = userId });

            return Task.FromResult(JsonSerializer.Deserialize<ThemeConfig>(jsonConfig) 
                                   ?? new ThemeConfig { UserId = userId });
        }
        catch (Exception ex)
        {
            _logger.Error("Error getting theme config for user {UserId}: {Error}", userId, ex.Message);
            return Task.FromResult(new ThemeConfig { UserId = userId });
        }
    }

    public async Task SetDarkModeAsync(bool isDarkMode)
    {
        try
        {
            if (_currentConfig == null)
            {
                throw new InvalidOperationException("Theme configuration is not initialized");
            }

            _currentConfig.IsDarkMode = isDarkMode;
            await ApplyThemeConfigAsync(_currentConfig);
            
            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = isDarkMode ? AppTheme.Dark : AppTheme.Light;
            
                // Update resources
                var themePath = isDarkMode ? "Resources/Themes/DarkTheme.xaml" : "Resources/Themes/LightTheme.xaml";
                await UpdateResourceDictionaryAsync(themePath);
            }
            else
            {
                _logger.Warning("Application.Current is null, cannot update application theme");
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Error setting dark mode to {IsDarkMode}: {Error}", isDarkMode, ex.Message);
            throw;
        }
    }

    public async Task SetPrimaryColorAsync(string colorHex)
    {
        ArgumentNullException.ThrowIfNull(colorHex);

        try
        {
            if (!Regex.IsMatch(colorHex, "^#(?:[0-9a-fA-F]{3}){1,2}$"))
                throw new ArgumentException("Invalid color hex code", nameof(colorHex));

            if (_currentConfig == null)
            {
                throw new InvalidOperationException("Theme configuration is not initialized");
            }

            _currentConfig.PrimaryColor = colorHex;
            await ApplyThemeConfigAsync(_currentConfig);

            if (Application.Current?.Resources != null)
            {
                Application.Current.Resources["Primary"] = Color.FromArgb(colorHex);
            }
            else
            {
                _logger.Warning("Application.Current or Resources is null, cannot update primary color");
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Error setting primary color to {ColorHex}: {Error}", colorHex, ex.Message);
            throw;
        }
    }

    public async Task ApplyThemeConfigAsync(ThemeConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        try
        {
            _currentConfig = config;
            var jsonConfig = JsonSerializer.Serialize(config);
            _preferences.Set($"theme_config_{config.UserId}", jsonConfig);

            ThemeChanged?.Invoke(this, config);
            await Task.CompletedTask; // Make the method truly async
        }
        catch (Exception ex)
        {
            _logger.Error("Error applying theme config: {Error}", ex.Message);
            throw;
        }
    }

    public async Task ResetToDefaultsAsync()
    {
        try
        {
            if (_currentConfig == null)
            {
                throw new InvalidOperationException("Theme configuration is not initialized");
            }

            var defaultConfig = new ThemeConfig
            {
                UserId = _currentConfig.UserId,
                IsDarkMode = false,
                PrimaryColor = DefaultPrimaryColor,
                ThemeType = ThemeType.System
            };

            await ApplyThemeConfigAsync(defaultConfig);
            await SetDarkModeAsync(false);
            await SetPrimaryColorAsync(DefaultPrimaryColor);
        }
        catch (Exception ex)
        {
            _logger.Error("Error resetting theme to defaults: {Error}", ex.Message);
            throw;
        }
    }

    private async Task UpdateResourceDictionaryAsync(string themePath)
    {
        ArgumentNullException.ThrowIfNull(themePath);

        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (Application.Current?.Resources?.MergedDictionaries == null)
                {
                    _logger.Warning("Application.Current, Resources, or MergedDictionaries is null");
                    return;
                }

                var mergedDicts = Application.Current.Resources.MergedDictionaries;
                mergedDicts.Clear();
                mergedDicts.Add(new ResourceDictionary { Source = new Uri(themePath, UriKind.Relative) });
            });
        }
        catch (Exception ex)
        {
            _logger.Error("Error updating resource dictionary: {Error}", ex.Message);
            throw;
        }
    }
    
    public void SetTheme(AppTheme theme)
    {
        try
        {
            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = theme;
            }
            else
            {
                _logger.Warning("Application.Current is null, cannot set application theme");
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Error setting theme: {Error}", ex.Message);
            throw;
        }
    }
} 
