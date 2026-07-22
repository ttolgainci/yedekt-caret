using MarbleWebProject.Services;

namespace MarbleWebProject.Helper;

/// <summary>Web → Marble API base URL (appsettings ApiService:BaseUrl).</summary>
public static class ApiClientConfig
{
    public static string GetBaseUrl()
    {
        var url = (Models.AppConfig.Storefront.StoreAuth?.EndPoint ?? "").Trim();
        if (string.IsNullOrEmpty(url))
            url = "http://localhost:5206";
        if (!url.EndsWith('/'))
            url += "/";
        return url;
    }

    public static void ApplyTo(WebRequest client)
    {
        client.Endpoint = GetBaseUrl();
    }
}
