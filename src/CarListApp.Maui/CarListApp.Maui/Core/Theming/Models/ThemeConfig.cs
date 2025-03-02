using System.Text.Json.Serialization;

namespace CarListApp.Maui.Core.Theming.Models;

public class ThemeConfig
{
    [JsonPropertyName("isDarkMode")]
    public bool IsDarkMode { get; set; }

    [JsonPropertyName("primaryColor")]
    public string PrimaryColor { get; set; } = "#6B4EFF";

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;
} 