namespace MarbleWebProject.Models;

public class AppConfig
{
    public static CDNServices CDNServices { get; set; } = new();
    public static StorefrontRuntimeConfig Storefront { get; set; } = new();

    /// <summary>StoreAuth.ProjectName kısayolu.</summary>
    public static string ProjectName => Storefront.StoreAuth.ProjectName;
}

public class CDNServices
{
    public string ContentUploads { get; set; } = "";
    public string ExtraContentPhotos { get; set; } = "";
    public string Favicon { get; set; } = "";
    public string Logo { get; set; } = "";
}
