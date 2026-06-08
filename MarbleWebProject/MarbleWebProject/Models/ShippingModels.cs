namespace MarbleWebProject.Models;

public sealed class ShippingCartItemRequest
{
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public sealed class ShippingCalculateRequest
{
    public int? CarrierId { get; set; }
    public int? CityId { get; set; }
    public int? TownId { get; set; }
    public string? PostalCode { get; set; }
    public List<ShippingCartItemRequest> CartItems { get; set; } = new();
}

public sealed class ShippingCalculateResponse
{
    public decimal TotalWeight { get; set; }
    public decimal TotalDesi { get; set; }
    public decimal ShippingPrice { get; set; }
    public int? CarrierId { get; set; }
    public string? CarrierName { get; set; }
    public int? CarrierRuleId { get; set; }
    public string Source { get; set; } = "manual";
}

public sealed class ShippingCarrierOption
{
    public int CarrierId { get; set; }
    public string CarrierName { get; set; } = "";
    public decimal ShippingPrice { get; set; }
    public decimal TotalDesi { get; set; }
    public decimal TotalWeight { get; set; }
    public int? CarrierRuleId { get; set; }
    public string Source { get; set; } = "manual";
}

public sealed class ShippingOptionsResponse
{
    public List<ShippingCarrierOption> Options { get; set; } = new();
    public int? DefaultCarrierId { get; set; }
}
