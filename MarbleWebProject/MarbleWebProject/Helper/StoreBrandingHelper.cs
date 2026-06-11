using MarbleWebProject.Helpers;
using MarbleWebProject.Models;

namespace MarbleWebProject.Helper;

public static class StoreBrandingHelper
{
    private const string DefaultLogoPath = "/assets/images/demos/demo-2/logo.png";

    public static string GetLogoUrl()
    {
        var path = AppConfig.Storefront?.General?.LogoPath;
        if (string.IsNullOrWhiteSpace(path))
            return DefaultLogoPath;

        var url = MediaUrlHelper.BuildBrandAsset(AppConfig.CDNServices.Logo, path);
        return string.IsNullOrWhiteSpace(url) ? DefaultLogoPath : url;
    }

    public static string GetFaviconUrl()
    {
        var path = AppConfig.Storefront?.General?.FaviconPath;
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        return MediaUrlHelper.BuildBrandAsset(AppConfig.CDNServices.Favicon, path);
    }

    public static string GetStoreName()
    {
        var name = AppConfig.Storefront?.General?.StoreName;
        if (!string.IsNullOrWhiteSpace(name))
            return name.Trim();
        return AppConfig.ProjectName;
    }
}
