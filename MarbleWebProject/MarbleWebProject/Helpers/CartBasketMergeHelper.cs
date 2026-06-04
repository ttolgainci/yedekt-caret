using MarbleWebProject.Models;

namespace MarbleWebProject.Helpers;

public static class CartBasketMergeHelper
{
    public static List<OrderBasket> MergeLines(IEnumerable<OrderBasket>? items)
    {
        if (items == null)
            return new List<OrderBasket>();

        return items
            .GroupBy(x => x.ProductID)
            .Select(g =>
            {
                var first = g.First();
                return new OrderBasket
                {
                    ID = first.ID,
                    ProductID = first.ProductID,
                    UserID = first.UserID,
                    LanguageCode = first.LanguageCode,
                    Name = first.Name,
                    Model = first.Model,
                    Price = first.Price,
                    Tax = first.Tax,
                    OrderID = first.OrderID,
                    Currency = first.Currency,
                    Image = first.Image,
                    Url = first.Url,
                    quantity = g.Sum(x => x.quantity ?? 0)
                };
            })
            .OrderBy(x => x.ProductID)
            .ToList();
    }

    public static List<CartModel> ToCartModels(IEnumerable<OrderBasket> merged)
    {
        return merged.Select(x => new CartModel
        {
            ProductID = x.ProductID,
            ProductName = x.Name ?? string.Empty,
            MainImage = x.Image ?? string.Empty,
            Url = x.Url ?? "#",
            Price = x.Price,
            CurrencyName = x.Currency ?? string.Empty,
            CartQuantity = x.quantity
        }).ToList();
    }

    public static BasketSetModel BuildBasketSetModel(IEnumerable<OrderBasket>? items)
    {
        var merged = MergeLines(items);
        var model = new BasketSetModel();
        if (merged.Count == 0)
            return model;

        model.CartList = ToCartModels(merged);
        var currency = merged.FirstOrDefault()?.Currency ?? string.Empty;
        var total = merged.Sum(c => (c.Price ?? 0) * (c.quantity ?? 0));
        model.Info.Total = "<span class='basket-total-price'>" + total.ToString("N2") + "</span> " + currency;
        model.Info.TotalQuantity = merged.Sum(c => c.quantity ?? 0);
        return model;
    }

    public static BasketReturnModel BuildReturnInfo(IEnumerable<OrderBasket>? items)
    {
        var merged = MergeLines(items);
        var info = new BasketReturnModel();
        if (merged.Count == 0)
            return info;

        var currency = merged.FirstOrDefault()?.Currency ?? string.Empty;
        var total = merged.Sum(c => (c.Price ?? 0) * (c.quantity ?? 0));
        info.TotalPrice = "<span class='basket-total-price'>" + total.ToString("N2") + "</span> " + currency;
        info.TotalQuantity = merged.Sum(c => c.quantity ?? 0);
        return info;
    }
}
