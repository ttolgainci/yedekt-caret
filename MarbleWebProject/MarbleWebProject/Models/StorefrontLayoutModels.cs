using System.Text.Json.Serialization;

namespace MarbleWebProject.Models;

public static class StorefrontWidgetCodes
{
    public const string MainBanner = "MAIN_BANNER";
    public const string ManufacturersBanner = "MANUFACTURERS_BANNER";
    public const string BannerGroup = "BANNER_GROUP";
    public const string ProductsTab = "PRODUCTS_TAB";
    public const string DealOfDay = "DEAL_OF_DAY";
    public const string TopSelling = "TOP_SELLING";
    public const string BlogPosts = "BLOG_POSTS";
    public const string FeaturedCategories = "FEATURED_CATEGORIES";
}

public sealed class StorefrontPageLayoutModel
{
    public string PageCode { get; set; } = "home";
    public List<StorefrontWidgetSlotModel> Widgets { get; set; } = new();
}

public sealed class StorefrontWidgetSlotModel
{
    public string WidgetCode { get; set; } = "";
    public string ViewComponentName { get; set; } = "";
    public int Order { get; set; }

    [JsonPropertyName("enabled")]
    public bool Enabled { get; set; } = true;

    public List<string> Sectors { get; set; } = new();
}

public sealed class CampaignActiveModel
{
    public int ID { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public int DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    public string? CouponCode { get; set; }
}

public sealed class PlaceGuestOrderApiRequest
{
    public string GuestUserId { get; set; } = "";
    public PlaceOrderApiRequest? Order { get; set; }
}

public sealed class PlaceOrderApiRequest
{
    public string LanguageCode { get; set; } = "tr";
    public string? ShippingAddressJson { get; set; }
    public string? BillingAddressJson { get; set; }
    public string ShippingMethod { get; set; } = "Standard";
    public int? CarrierID { get; set; }
    public int? ShippingCityID { get; set; }
    public int? ShippingTownID { get; set; }
    public string? PostalCode { get; set; }
    public string? CouponCode { get; set; }
    public int? CustomerId { get; set; }
    public string? PaymentMethod { get; set; }
    public int? BankAccountId { get; set; }
}

public sealed class PlaceOrderApiResponse
{
    public int OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public decimal GrandTotal { get; set; }
    public string CurrencyCode { get; set; } = "";
    public string CurrencySymbol { get; set; } = "";
    public decimal CampaignDiscount { get; set; }
    public DateTime? PaymentDueAt { get; set; }
    public int? BankAccountId { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountHolder { get; set; }
    public string? BankIban { get; set; }
}
