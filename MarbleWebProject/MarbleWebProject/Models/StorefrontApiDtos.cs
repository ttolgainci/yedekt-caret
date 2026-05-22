namespace MarbleWebProject.Models
{
    public class PlaceOrderRequestDto
    {
        public string LanguageCode { get; set; } = string.Empty;
        public int? StoreID { get; set; }
        public string? ShippingAddressJson { get; set; }
        public string? BillingAddressJson { get; set; }
        public string ShippingMethod { get; set; } = "Standard";
    }

    public class PlaceGuestOrderRequestDto
    {
        public string GuestUserId { get; set; } = string.Empty;
        public PlaceOrderRequestDto? Order { get; set; }
    }

    public class PlaceOrderResponseDto
    {
        public int OrderId { get; set; }
        public decimal GrandTotal { get; set; }
        public string CurrencyCode { get; set; } = string.Empty;
    }

    public class ProductVariantListItemDto
    {
        public int Id { get; set; }
        public int ProductID { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string? Barcode { get; set; }
        public int? Quantity { get; set; }
        public decimal? PriceDelta { get; set; }
        public int SortOrder { get; set; }
        public bool Status { get; set; }
    }

    public class VehicleMakeListItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public bool Status { get; set; }
    }

    public class VehicleModelListItemDto
    {
        public int Id { get; set; }
        public int VehicleMakeID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public bool Status { get; set; }
    }

    public class VehicleCompatibilityListItemDto
    {
        public int Id { get; set; }
        public int ProductID { get; set; }
        public int VehicleEngineID { get; set; }
        public string? VehicleMakeName { get; set; }
        public string? VehicleModelName { get; set; }
        public string? GenerationName { get; set; }
        public int? GenerationStartYear { get; set; }
        public int? GenerationEndYear { get; set; }
        public string EngineCode { get; set; } = string.Empty;
        public string? FuelType { get; set; }
        public int? PowerHp { get; set; }
        public string Position { get; set; } = string.Empty;
        public string? Note { get; set; }
    }
}
