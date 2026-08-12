namespace MarbleWebProject.Models;

using MarbleWebProject.Helpers;

public class WishlistItemModel
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string MainImage { get; set; } = string.Empty;
    public string Url { get; set; } = "#";
    public decimal? Price { get; set; }
    public string CurrencyName { get; set; } = string.Empty;
    public int? StockQuantity { get; set; }
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public decimal? OriginalPrice { get; set; }
    public decimal? DiscountPercent { get; set; }
    public bool HasDiscount { get; set; }

    public bool IsLowStock => StoreLowStockAlert.IsLowStock(StockQuantity);
    public bool IsInStock => StockQuantity is > 0;
    public bool IsOutOfStock => StockQuantity is null or <= 0;
}

public class WishlistSetModel
{
    public List<WishlistItemModel> Items { get; set; } = new();
    public int TotalCount => Items.Count;

    public IReadOnlyList<WishlistCategoryFilterModel> CategoryFilters =>
        Items
            .Where(x => x.CategoryID > 0 && !string.IsNullOrWhiteSpace(x.CategoryName))
            .GroupBy(x => x.CategoryID)
            .Select(g => new WishlistCategoryFilterModel
            {
                CategoryID = g.Key,
                CategoryName = g.First().CategoryName,
                Count = g.Count()
            })
            .OrderBy(x => x.CategoryName)
            .ToList();
}

public sealed class WishlistCategoryFilterModel
{
    public int CategoryID { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class WishlistAllRequest
{
    public string? UserID { get; set; }
    public string? LanguageCode { get; set; }
}

public class WishlistToggleRequest
{
    public int ProductID { get; set; }
    public string? UserID { get; set; }
    public string? LanguageCode { get; set; }
    public string? Url { get; set; }
}

public class WishlistRemoveRequest
{
    public int ProductID { get; set; }
    public string? UserID { get; set; }
    public string? LanguageCode { get; set; }
}

public class WishlistToggleResultModel
{
    public bool IsInWishlist { get; set; }
    public int TotalCount { get; set; }
    public List<WishlistItemDto> Items { get; set; } = new();
}

public class WishlistItemDto
{
    public int ProductID { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string Image { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
}
