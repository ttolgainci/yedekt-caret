using MarbleWebProject.Helpers;
using MarbleWebProject.Models;

namespace MarbleWebProject.Models.Options;

/// <summary>
/// Vitrin kurulum + servis hesabı (appsettings StoreAuth).
/// LanguageCode / LanguageCulture / MarketCode / EndPoint runtime'da set edilir (json'da yok).
/// </summary>
public sealed class StoreAuthOptions
{
    public const string SectionName = "StoreAuth";

    public string UserName { get; set; } = "";
    public string Password { get; set; } = "";
    public string CustomName { get; set; } = "";

    /// <summary>Body CSS sınıfı — store-themes.css (örn. store-theme-green).</summary>
    public string Theme { get; set; } = StoreThemeHelper.DefaultTheme;

    public StoreSectorType StoreSectorType { get; set; } = StoreSectorType.Apparel;
    public string SectorCode { get; set; } = "";
    public string ProjectName { get; set; } = "";

    /// <summary>Header ana menüde gösterilecek üst kategori sayısı.</summary>
    public int HeaderNavMaxVisibleCategories { get; set; } = 9;

    /// <summary>Aynı gün kargo tahmini: kesim saati (yerel). Negatif = kapalı.</summary>
    public int SameDayShippingCutoffHour { get; set; } = 16;

    /// <summary>Aynı gün kargo tahmini: kesim dakikası.</summary>
    public int SameDayShippingCutoffMinute { get; set; } = 0;

    /// <summary>Ürün detayda «Tahmini Kargo» satırını göster.</summary>
    public bool ShowSameDayShippingEstimate { get; set; } = true;

    /// <summary>API base URL — Program.cs ApiService:BaseUrl'den doldurulur.</summary>
    public string EndPoint { get; set; } = "";

    /// <summary>Login sonrası / dil değişiminde set edilir.</summary>
    public string LanguageCode { get; set; } = "";
    public string LanguageCulture { get; set; } = "";
    public int? MarketCode { get; set; }

    public bool IsAutoParts => ProjectSector.IsAutoParts(SectorCode);
    public bool IsApparelOrElectronics => !IsAutoParts;
}
