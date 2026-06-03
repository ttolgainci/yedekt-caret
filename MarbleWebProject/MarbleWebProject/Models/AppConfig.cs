namespace MarbleWebProject.Models;

public class AppConfig
{
    public static CMSService CMSService { get; set; } = new();
    public static CDNServices CDNServices { get; set; } = new();
    public static ProjectServiceSettings ProjectService { get; set; } = new();
    public static StorefrontRuntimeConfig Storefront { get; set; } = new();

    /// <summary>ProjectService.ProjectName kısayolu.</summary>
    public static string ProjectName => ProjectService.ProjectName;
}

public class CMSService
{
    public string EndPoint { get; set; } = "";
    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string CustomName { get; set; } = "";
    public int? MarketCode { get; set; }
    public string Theme { get; set; } = "";
    public string LanguageCode { get; set; } = "";
    public string LanguageCulture { get; set; } = "";
}

public class CDNServices
{
    public string ContentUploads { get; set; } = "";
    public string ExtraContentPhotos { get; set; } = "";
    public string Favicon { get; set; } = "";
}
