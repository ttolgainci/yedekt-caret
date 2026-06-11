using MarbleWebProject.Models;



namespace MarbleWebProject.Helpers;



public static class MediaUrlHelper

{

    /// <summary>CDN tabanı (JS / sepet görselleri için).</summary>

    public static string GetUploadsBaseUrl()

    {

        var baseUrl = ResolveUploadsBase();

        return NormalizeLocalDevUrl(baseUrl);

    }



    /// <summary>Ürün görseli; boşsa CDNServices.ExtraContentPhotos varsayılanı.</summary>
    public static string BuildProductImage(string? relativePath)
    {
        if (!string.IsNullOrWhiteSpace(relativePath))
            return Build(relativePath);

        return ResolveDefaultProductImage();
    }

    /// <summary>Logo / favicon gibi marka varlıkları (CMS CDNServices.Logo / Favicon tabanı).</summary>
    public static string BuildBrandAsset(string? dedicatedBase, string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return string.Empty;

        var path = relativePath.Trim().Replace('\\', '/');
        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return NormalizeLocalDevUrl(path);

        var baseUrl = ResolveDedicatedBase(dedicatedBase);
        if (string.IsNullOrEmpty(baseUrl))
            return path.StartsWith('/') ? path : "/" + path.TrimStart('/');

        return NormalizeLocalDevUrl(baseUrl + path.TrimStart('/'));
    }

    public static string Build(string? relativePath)

    {

        if (string.IsNullOrWhiteSpace(relativePath))

            return string.Empty;



        var path = relativePath.Trim().Replace('\\', '/');

        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase)

            || path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))

            return NormalizeLocalDevUrl(path);



        var baseUrl = ResolveUploadsBase();

        if (string.IsNullOrEmpty(baseUrl))

            return path.StartsWith('/') ? path : "/" + path.TrimStart('/');



        var project = AppConfig.ProjectName?.Trim() ?? "";

        var normalizedPath = path.TrimStart('/');



        if (!string.IsNullOrEmpty(project))

        {

            var projectPrefix = project + "/";

            if (normalizedPath.StartsWith(projectPrefix, StringComparison.OrdinalIgnoreCase))

                return NormalizeLocalDevUrl(baseUrl + normalizedPath);



            if (normalizedPath.StartsWith("Upload/", StringComparison.OrdinalIgnoreCase))

                return NormalizeLocalDevUrl(baseUrl + project + "/" + normalizedPath);

        }



        if (normalizedPath.Contains("/Upload/", StringComparison.OrdinalIgnoreCase))

            return NormalizeLocalDevUrl(baseUrl + normalizedPath);



        if (!string.IsNullOrEmpty(project))

            return NormalizeLocalDevUrl(baseUrl + project + "/Upload/" + normalizedPath);



        return NormalizeLocalDevUrl(baseUrl + normalizedPath);

    }



    private static string ResolveDefaultProductImage()
    {
        var fallback = AppConfig.CDNServices?.ExtraContentPhotos?.Trim() ?? "";
        if (string.IsNullOrEmpty(fallback))
            return string.Empty;

        if (fallback.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || fallback.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return NormalizeLocalDevUrl(fallback);

        if (fallback.StartsWith('/'))
            return fallback;

        return Build(fallback);
    }

    private static string ResolveDedicatedBase(string? dedicated)
    {
        var raw = string.IsNullOrWhiteSpace(dedicated)
            ? AppConfig.CDNServices?.ContentUploads?.Trim() ?? ""
            : dedicated.Trim();
        if (string.IsNullOrEmpty(raw))
            return string.Empty;
        return raw.EndsWith('/') ? raw : raw + "/";
    }

    private static string ResolveUploadsBase()

    {

        var cdn = AppConfig.CDNServices?.ContentUploads?.Trim() ?? "";

        if (string.IsNullOrEmpty(cdn))

            return string.Empty;



        if (!cdn.EndsWith('/'))

            cdn += "/";



        var project = AppConfig.ProjectName?.Trim() ?? "";

        if (string.IsNullOrEmpty(project))

            return cdn;



        if (cdn.Contains($"/{project}/", StringComparison.OrdinalIgnoreCase))

            return cdn;



        return cdn;

    }



    /// <summary>Dev: https://localhost CDN/API → http (mixed content ve SSL hatasını önler).</summary>

    private static string NormalizeLocalDevUrl(string url)

    {

        if (string.IsNullOrWhiteSpace(url))

            return url;



        if (url.StartsWith("https://localhost", StringComparison.OrdinalIgnoreCase)

            || url.StartsWith("https://127.0.0.1", StringComparison.OrdinalIgnoreCase))

            return "http://" + url[8..];



        return url;

    }

}


