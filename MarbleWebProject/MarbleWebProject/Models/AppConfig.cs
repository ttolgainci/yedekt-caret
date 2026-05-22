namespace MarbleWebProject.Models
{
    public class AppConfig
    {
        public static CMSService CMSService { get; set; }
        public static CDNServices CDNServices { get; set; }
        /// <summary>Ürün URL'leri SEO indeks + catch-all ile çözülür; startup'ta N route kaydı yapılmaz.</summary>
        public static bool UseIndexedProductRouting { get; set; } = true;

        /// <summary>Storefront API kök URL (örn. https://localhost:7198/).</summary>
        public static string StorefrontApiBaseUrl { get; set; } = "https://localhost:7198/";

        /// <summary><c>api/orders/place-guest</c> için API ile aynı olmalı; boşsa misafir sipariş kapalı.</summary>
        public static string StorefrontGuestCheckoutKey { get; set; } = string.Empty;

        /// <summary>API AgencyFeatureProfile veya <c>Storefront:LayoutVersion</c>; tema seçimi için <c>body</c> sınıfında kullanılır.</summary>
        public static string StorefrontLayoutVersion { get; set; } = "v1";
    }
    public class CMSService
    {
        public string EndPoint { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string CustomName { get; set; }
        public int? MarketCode { get; set; }
        public string Theme { get; set; }
        public string LanguageCode { get; set; }
        public string LanguageCulture { get; set; }
    }
    public class CDNServices
    {
        public string ContentUploads { get; set; }
        public string ExtraContentPhotos { get; set; }
        public string Favicon { get; set; }

    }
}
