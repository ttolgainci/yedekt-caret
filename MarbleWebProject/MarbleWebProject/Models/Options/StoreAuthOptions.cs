using MarbleWebProject.Helpers;

namespace MarbleWebProject.Models.Options;

/// <summary>Vitrin servis hesabı (Users tablosu — CMS admin değil).</summary>
public sealed class StoreAuthOptions
{
    public const string SectionName = "StoreAuth";

    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string CustomName { get; set; } = "";

    /// <summary>Body CSS sınıfı — store-themes.css içindeki tema (örn. store-theme-green).</summary>
    public string Theme { get; set; } = StoreThemeHelper.DefaultTheme;
}
