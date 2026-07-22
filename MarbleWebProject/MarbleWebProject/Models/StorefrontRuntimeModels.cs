using System.Text.Json.Serialization;
using MarbleWebProject.Models.Options;

namespace MarbleWebProject.Models;

public sealed class StorefrontRuntimeConfig
{
    /// <summary>Kurulum + login runtime state (StoreAuth).</summary>
    public StoreAuthOptions StoreAuth { get; set; } = new();

    public string LayoutVersion { get; set; } = "v1";

    /// <summary>Key = ViewComponentName (DB), Value = IsActive</summary>
    public Dictionary<string, bool> Header { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Key = ViewComponentName (DB), Value = IsActive</summary>
    public Dictionary<string, bool> Footer { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public StorefrontPageLayoutModel Home { get; set; } = new();

    public string SearchResultLayout { get; set; } = "grid4";

    public StoreGeneralSettingsModel General { get; set; } = new();

    public List<StoreCurrencyModel> Currencies { get; set; } = new();
}

public sealed class StorefrontPublishedLayoutModel
{
    public string LayoutVersion { get; set; } = "v1";
    public string SearchResultLayout { get; set; } = "grid4";
    public List<StorefrontPublishedWidgetModel> Header { get; set; } = new();
    public List<StorefrontPublishedWidgetModel> Footer { get; set; } = new();
    public StorefrontPageLayoutModel Home { get; set; } = new();
}

public sealed class StorefrontPublishedWidgetModel
{
    public string WidgetCode { get; set; } = "";
    public string ViewComponentName { get; set; } = "";

    [JsonPropertyName("isActive")]
    public bool IsActive { get; set; }

    public int SortOrder { get; set; }
}
