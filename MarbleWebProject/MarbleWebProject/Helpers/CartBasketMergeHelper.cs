using MarbleWebProject.Models;

namespace MarbleWebProject.Helpers;

/// <summary>Sepet/checkout toplamları — gösterilen ürün fiyatları KDV dahildir.</summary>
public static class CartBasketMergeHelper
{
    public static decimal ComputeGrossSubtotal(IEnumerable<CartModel> lines) =>
        lines.Sum(line => (line.Price ?? 0) * (line.CartQuantity ?? 0));

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
                    ProductVariantID = first.ProductVariantID,
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
                    StockQuantity = g.Min(x => x.StockQuantity ?? int.MaxValue) is var min && min < int.MaxValue ? min : first.StockQuantity,
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
            CartQuantity = x.quantity,
            TaxPercent = x.Tax,
            StockQuantity = x.StockQuantity
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
        var total = ComputeGrossSubtotal(model.CartList);
        model.Info.CurrencyName = currency;
        model.Info.Total = "<span class='basket-total-price'>" + CurrencyDisplayHelper.FormatAmount(total, currency) + "</span>";
        model.Info.TotalQuantity = merged.Sum(c => c.quantity ?? 0);
        model.Info.ItemCount = merged.Count;
        return model;
    }

    public static BasketReturnModel BuildReturnInfo(IEnumerable<OrderBasket>? items) =>
        BuildReturnInfo(BuildBasketSetModel(items));

    public static BasketReturnModel BuildReturnInfo(BasketSetModel model)
    {
        var info = new BasketReturnModel();
        if (model.CartList.Count == 0)
            return info;

        var currency = model.Info.CurrencyName ?? string.Empty;
        info.SubtotalPrice = AppendCurrencyIfMissing(model.Info.Subtotal, currency);
        info.DrawerTotalPrice = AppendCurrencyIfMissing(model.Info.Total, currency);
        info.TotalPrice = AppendCurrencyIfMissing(model.Info.GrandTotal ?? model.Info.Total, currency);
        info.TotalQuantity = model.Info.TotalQuantity;
        info.ItemCount = model.Info.ItemCount ?? model.CartList.Count;
        info.ShippingPrice = model.Info.ShippingPrice;
        info.CarrierName = model.Info.CarrierName;
        info.TotalDesi = model.Info.TotalDesi;
        info.CurrencyName = currency;
        return info;
    }

    private static string AppendCurrencyIfMissing(string? value, string currency)
    {
        if (string.IsNullOrWhiteSpace(value) || string.IsNullOrWhiteSpace(currency))
            return value ?? string.Empty;

        return value.Contains(currency, StringComparison.OrdinalIgnoreCase)
            ? value
            : value + " " + currency;
    }
}
