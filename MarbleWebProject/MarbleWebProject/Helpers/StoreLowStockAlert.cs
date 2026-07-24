namespace MarbleWebProject.Helpers;

/// <summary>
/// Ürün detayı ile aynı kural: stok &gt; 0 ve stok ≤ 3 → düşük stok uyarısı.
/// </summary>
public static class StoreLowStockAlert
{
    public const int Threshold = 3;

    public static bool IsLowStock(int? stockQuantity) =>
        stockQuantity is > 0 and <= Threshold;

    public static List<LowStockAlertDto> PickAll(
        IEnumerable<(int ProductId, string Name, string Image, string Url, int? StockQuantity)> items)
    {
        var result = new List<LowStockAlertDto>();
        foreach (var item in items)
        {
            if (!IsLowStock(item.StockQuantity))
                continue;

            result.Add(new LowStockAlertDto
            {
                ProductID = item.ProductId,
                Name = item.Name ?? string.Empty,
                Image = item.Image ?? string.Empty,
                Url = string.IsNullOrWhiteSpace(item.Url) ? "#" : item.Url,
                StockQuantity = item.StockQuantity!.Value
            });
        }

        return result;
    }

    public static LowStockAlertDto? PickFirst(
        IEnumerable<(int ProductId, string Name, string Image, string Url, int? StockQuantity)> items) =>
        PickAll(items).FirstOrDefault();

    public static object? BuildPayload(
        IEnumerable<(int ProductId, string Name, string Image, string Url, int? StockQuantity)> items,
        string ctaUrl)
    {
        var list = PickAll(items);
        if (list.Count == 0)
            return null;

        return new
        {
            ctaUrl,
            items = list.Select(x => new
            {
                productID = x.ProductID,
                name = x.Name,
                image = x.Image,
                url = x.Url,
                stockQuantity = x.StockQuantity
            })
        };
    }
}

public sealed class LowStockAlertDto
{
    public int ProductID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Image { get; set; } = string.Empty;
    public string Url { get; set; } = "#";
    public int StockQuantity { get; set; }
}
