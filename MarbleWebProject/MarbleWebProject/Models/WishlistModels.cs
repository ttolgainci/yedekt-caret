namespace MarbleWebProject.Models;

public class WishlistItemModel
{
    public int ProductID { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string MainImage { get; set; } = string.Empty;
    public string Url { get; set; } = "#";
    public decimal? Price { get; set; }
    public string CurrencyName { get; set; } = string.Empty;
}

public class WishlistSetModel
{
    public List<WishlistItemModel> Items { get; set; } = new();
    public int TotalCount => Items.Count;
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
