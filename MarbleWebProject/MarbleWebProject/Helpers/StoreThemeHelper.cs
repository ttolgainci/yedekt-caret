using System.Text.RegularExpressions;

namespace MarbleWebProject.Helpers;

/// <summary>appsettings StoreAuth:Theme — body CSS sınıfı (örn. store-theme-green).</summary>
public static class StoreThemeHelper
{
    public const string DefaultTheme = "store-theme-green";

    private static readonly Regex SafeThemeClass = new(
        @"^store-theme-[a-z0-9-]+$",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static string Resolve(string? theme)
    {
        var value = (theme ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(value))
            return DefaultTheme;

        return SafeThemeClass.IsMatch(value) ? value : DefaultTheme;
    }
}
