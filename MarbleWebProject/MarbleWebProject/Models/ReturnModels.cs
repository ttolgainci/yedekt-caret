namespace MarbleWebProject.Models;

public sealed class ReturnRequestListItemModel
{
    public int ID { get; set; }
    public int ShopOrderID { get; set; }
    public int Status { get; set; }
    public string StatusText { get; set; } = "";
    public string? Reason { get; set; }
    public DateTime CreatedAt { get; set; }
}

public sealed class ReturnRequestDetailModel
{
    public int ID { get; set; }
    public int ShopOrderID { get; set; }
    public int Status { get; set; }
    public string StatusText { get; set; } = "";
    public string? Reason { get; set; }
    public string? AdminNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<ReturnRequestLineModel> Lines { get; set; } = new();
}

public sealed class ReturnRequestLineModel
{
    public int ID { get; set; }
    public int ShopOrderLineID { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
    public string? ProductNameSnapshot { get; set; }
}

public sealed class StoreReturnCreateForm
{
    public string? Reason { get; set; }
    public List<StoreReturnLineForm> Lines { get; set; } = new();
}

public sealed class StoreReturnLineForm
{
    public int ShopOrderLineID { get; set; }
    public int Quantity { get; set; }
    public string? Note { get; set; }
}
