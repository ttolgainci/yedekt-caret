using MarbleWebProject.Models;

namespace MarbleWebProject.Helpers;

public static class WishlistHelper
{
    public static WishlistSetModel BuildSetModel(IEnumerable<WishlistItemDto>? items)
    {
        var model = new WishlistSetModel();
        if (items == null)
            return model;

        model.Items = items.Select(x => new WishlistItemModel
        {
            ProductID = x.ProductID,
            ProductName = x.Name ?? string.Empty,
            MainImage = x.Image ?? string.Empty,
            Url = string.IsNullOrWhiteSpace(x.Url) ? "#" : x.Url,
            Price = x.Price,
            CurrencyName = x.Currency ?? string.Empty
        }).ToList();

        return model;
    }

    public static WishlistSetModel BuildSetModelFromApiItems(IEnumerable<WishlistApiItem>? items)
    {
        var model = new WishlistSetModel();
        if (items == null)
            return model;

        model.Items = items.Select(x => new WishlistItemModel
        {
            ProductID = x.ProductID,
            ProductName = x.Name ?? string.Empty,
            MainImage = x.Image ?? string.Empty,
            Url = string.IsNullOrWhiteSpace(x.Url) ? "#" : x.Url,
            Price = x.Price,
            CurrencyName = x.Currency ?? string.Empty
        }).ToList();

        return model;
    }
}

public class WishlistApiItem
{
    public int ProductID { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string Image { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
}
