namespace MarbleWebProject.Models;

/// <summary>CMS /configuration/settings/general — vitrin okuma modeli.</summary>
public sealed class StoreGeneralSettingsModel
{
    public string StoreName { get; set; } = "";
    public string? LogoPath { get; set; }
    public string? FaviconPath { get; set; }
    public string? FacebookUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? YoutubeUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string SeoTitleSeparator { get; set; } = "|";
    public string? DefaultPageTitle { get; set; }
    public string? DefaultMetaKeywords { get; set; }
    public string? DefaultMetaDescription { get; set; }
    public string? HomePageTitle { get; set; }
    public string? HomePageMetaDescription { get; set; }
    public string? CustomHeadHtml { get; set; }
}
