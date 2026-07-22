namespace MarbleWebProject.Models;

public sealed class ShopOrderListItemModel
{
    public int Id { get; set; }
    public string? OrderNumber { get; set; }
    public int OrderStatus { get; set; }
    public int PaymentStatus { get; set; }
    public decimal GrandTotal { get; set; }
    public string CurrencyCode { get; set; } = "";
    public string CurrencySymbol { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public int LineCount { get; set; }
    public int TotalQuantity { get; set; }
    public List<string> Thumbnails { get; set; } = new();
}

public sealed class ShopOrderDetailModel
{
    public int Id { get; set; }
    public string? OrderNumber { get; set; }
    public int OrderStatus { get; set; }
    public int PaymentStatus { get; set; }
    public string? PaymentMethod { get; set; }
    public decimal SubTotal { get; set; }
    public decimal ShippingTotal { get; set; }
    public decimal TaxTotal { get; set; }
    public decimal CampaignDiscount { get; set; }
    public decimal GrandTotal { get; set; }
    public string CurrencyCode { get; set; } = "";
    public string CurrencySymbol { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public OrderAddressSnapshotModel? ShippingAddress { get; set; }
    public OrderAddressSnapshotModel? BillingAddress { get; set; }
    public OrderPaymentSnapshotModel? Payment { get; set; }
    public List<ShopOrderLineModel> Lines { get; set; } = new();
    public int LineCount { get; set; }
    public int TotalQuantity { get; set; }
    public ShopOrderShipmentSnapshotModel? Shipment { get; set; }
    public BankTransferOrderInfoModel? BankTransfer { get; set; }
}

public sealed class ShopOrderLineModel
{
    public int Id { get; set; }
    public int ProductID { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public decimal VatRate { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string ProductNameSnapshot { get; set; } = "";
    public string? SkuSnapshot { get; set; }
    public string? Picture { get; set; }
}

public sealed class ShopOrderShipmentSnapshotModel
{
    public string? ShipmentNumber { get; set; }
    public string? TrackingNumber { get; set; }
    public string? TrackingUrl { get; set; }
    public string? CarrierName { get; set; }
    public string Status { get; set; } = "";
    public string StatusText { get; set; } = "";
    public decimal ShippingPrice { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? LastTrackedAtUtc { get; set; }
    public List<ShopOrderShipmentTrackingEventModel> TrackingEvents { get; set; } = new();
}

public sealed class ShopOrderShipmentTrackingEventModel
{
    public DateTimeOffset AtUtc { get; set; }
    public string Code { get; set; } = "";
    public string Description { get; set; } = "";
}

public sealed class ShipmentTrackingResultModel
{
    public string? TrackingNumber { get; set; }
    public string Status { get; set; } = "";
    public DateTimeOffset CheckedAtUtc { get; set; }
    public List<ShopOrderShipmentTrackingEventModel> Events { get; set; } = new();
}

public sealed class OrderAddressSnapshotModel
{
    public string? ContactFirstName { get; set; }
    public string? ContactLastName { get; set; }
    public string? ContactPhone { get; set; }
    public string? Email { get; set; }
    public string? CompanyName { get; set; }
    public string? TaxOffice { get; set; }
    public string? TaxNumber { get; set; }
    public string? CountryName { get; set; }
    public string? CityName { get; set; }
    public string? TownName { get; set; }
    public string AddressLine1 { get; set; } = "";
    public string? AddressLine2 { get; set; }
    public string? PostalCode { get; set; }
    public string? DeliveryInstructions { get; set; }
    public string DisplayLine { get; set; } = "";
}

public sealed class OrderPaymentSnapshotModel
{
    public string PaymentMethod { get; set; } = "";
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; } = "";
    public int Status { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? ReceiptPath { get; set; }
    public string? ReceiptUrl { get; set; }
    public DateTime? ReceiptUploadedAt { get; set; }
    public int BankVerificationStatus { get; set; }
    public string BankVerificationStatusText { get; set; } = "";
}

public sealed class MergeCartForm
{
    public string? GuestUserId { get; set; }
    public string? LanguageCode { get; set; }
}

public sealed class CheckoutPaymentProviderModel
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";
    public bool IsDefault { get; set; }
}
