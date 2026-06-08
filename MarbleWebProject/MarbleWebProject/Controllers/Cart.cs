using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http;

namespace MarbleWebProject.Controllers;

public class Cart : Controller
{
    private readonly IStoreBasketApi _basket;
    private readonly IStoreAuthService _auth;
    private readonly IStoreShippingApi _shipping;
    private readonly IBasketUserIdProvider _basketUserId;

    public Cart(
        IStoreBasketApi basket,
        IStoreAuthService auth,
        IStoreShippingApi shipping,
        IBasketUserIdProvider basketUserId)
    {
        _basket = basket;
        _auth = auth;
        _shipping = shipping;
        _basketUserId = basketUserId;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var userGuid = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        var session = await _auth.GetSessionAsync(cancellationToken);
        var model = await LoadBasketSetModelAsync(userGuid, session.LanguageCode, cancellationToken);
        await ApplyShippingQuoteAsync(model, cancellationToken);
        return View(model);
    }

    [HttpGet]
    [Route("Cart/Snapshot")]
    public async Task<IActionResult> Snapshot(CancellationToken cancellationToken = default)
    {
        var userGuid = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(userGuid))
            return Json(new { CartList = Array.Empty<object>(), Info = CartBasketMergeHelper.BuildReturnInfo(new BasketSetModel()) });

        var session = await _auth.GetSessionAsync(cancellationToken);
        return await BasketJsonAsync(userGuid, session.LanguageCode, cancellationToken);
    }

    [HttpPost]
    [Route("Cart/CalculateShipping")]
    public async Task<IActionResult> CalculateShipping([FromBody] ShippingCalculateRequest? request, CancellationToken cancellationToken)
    {
        var userGuid = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        var session = await _auth.GetSessionAsync(cancellationToken);
        var model = await LoadBasketSetModelAsync(userGuid, session.LanguageCode, cancellationToken);

        if (request == null)
            request = new ShippingCalculateRequest();

        request.CartItems = model.CartList
            .Select(x => new ShippingCartItemRequest
            {
                ProductId = x.ProductID,
                Quantity = x.CartQuantity is < 1 ? 1 : x.CartQuantity!.Value
            })
            .ToList();

        var quote = await _shipping.CalculateAsync(request, cancellationToken);
        if (quote == null)
            return NotFound(new { message = "Kargo ücreti hesaplanamadı." });

        return Json(quote);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int ProductID, string Url, int? CartQuantity, CancellationToken cancellationToken = default)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var userGuid = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        var qty = CartQuantity is < 1 ? 1 : CartQuantity!.Value;

        await _basket.GetByIdBasketAsync(new BasketRequest
        {
            LanguageCode = session.LanguageCode,
            ProductID = ProductID,
            CartQuantity = qty,
            Url = Url ?? string.Empty,
            UserID = userGuid
        }, cancellationToken);

        return await BasketJsonAsync(userGuid, session.LanguageCode, cancellationToken);
    }

    [HttpPost]
    public async Task<IActionResult> ChangeCartQuantity(int ProductID, string Url, int Delta, CancellationToken cancellationToken = default)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var userGuid = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        var all = await _basket.GetBasketAllAsync(new BasketAllRequest { UserID = userGuid, LanguageCode = session.LanguageCode }, cancellationToken);
        var merged = all.Status && all.Data != null
            ? CartBasketMergeHelper.MergeLines(all.Data)
            : new List<OrderBasket>();

        var line = merged.FirstOrDefault(x => x.ProductID == ProductID);
        var currentQty = line?.quantity ?? 0;
        var newQty = currentQty + Delta;

        if (newQty < 1)
        {
            await _basket.DeleteProductFromCartAsync(new DeleteBasketRequest
            {
                ProductID = ProductID,
                UserID = userGuid,
                LanguageCode = session.LanguageCode
            }, cancellationToken);
        }
        else
        {
            await _basket.GetByIdBasketAsync(new BasketRequest
            {
                LanguageCode = session.LanguageCode,
                ProductID = ProductID,
                CartQuantity = Delta,
                IsDelta = true,
                Url = Url ?? line?.Url ?? string.Empty,
                UserID = userGuid
            }, cancellationToken);
        }

        return await BasketJsonAsync(userGuid, session.LanguageCode, cancellationToken);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteFromCart(int ProductID, CancellationToken cancellationToken = default)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var userGuid = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);

        await _basket.DeleteProductFromCartAsync(new DeleteBasketRequest
        {
            ProductID = ProductID,
            UserID = userGuid,
            LanguageCode = session.LanguageCode
        }, cancellationToken);

        return await BasketJsonAsync(userGuid, session.LanguageCode, cancellationToken);
    }

    [HttpPost]
    public async Task<IActionResult> ClearCart(CancellationToken cancellationToken = default)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var userGuid = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        var basketRequest = new BasketAllRequest
        {
            UserID = userGuid,
            LanguageCode = session.LanguageCode
        };

        var cleared = false;
        try
        {
            await _basket.ClearCartAsync(basketRequest, cancellationToken);
            cleared = true;
        }
        catch (HttpRequestException)
        {
            cleared = false;
        }

        if (!cleared)
        {
            await ClearCartByDeletingLinesAsync(userGuid, session.LanguageCode, cancellationToken);
        }

        return await BasketJsonAsync(userGuid, session.LanguageCode, cancellationToken);
    }

    private async Task ClearCartByDeletingLinesAsync(string userGuid, string languageCode, CancellationToken cancellationToken)
    {
        var all = await _basket.GetBasketAllAsync(new BasketAllRequest
        {
            UserID = userGuid,
            LanguageCode = languageCode
        }, cancellationToken);

        if (!all.Status || all.Data == null)
            return;

        foreach (var line in CartBasketMergeHelper.MergeLines(all.Data))
        {
            await _basket.DeleteProductFromCartAsync(new DeleteBasketRequest
            {
                ProductID = line.ProductID,
                UserID = userGuid,
                LanguageCode = languageCode
            }, cancellationToken);
        }
    }

    private async Task<BasketSetModel> LoadBasketSetModelAsync(string userGuid, string languageCode, CancellationToken cancellationToken)
    {
        var routeResponse = await _basket.GetBasketAllAsync(new BasketAllRequest { UserID = userGuid, LanguageCode = languageCode }, cancellationToken);
        return routeResponse.Status && routeResponse.Data != null
            ? CartBasketMergeHelper.BuildBasketSetModel(routeResponse.Data)
            : new BasketSetModel();
    }

    private async Task ApplyShippingQuoteAsync(BasketSetModel model, CancellationToken cancellationToken)
    {
        if (model.CartList.Count == 0)
        {
            model.Info.ShippingPrice = null;
            model.Info.CarrierName = null;
            model.Info.TotalDesi = null;
            model.Info.CarrierId = null;
            model.Info.Subtotal = string.Empty;
            model.Info.GrandTotal = string.Empty;
            model.Info.Total = string.Empty;
            model.Info.CurrencyName = string.Empty;
            return;
        }

        var subtotal = model.CartList.Sum(c => (c.Price ?? 0) * (c.CartQuantity ?? 0));
        var currency = model.CartList.FirstOrDefault()?.CurrencyName ?? string.Empty;
        model.Info.CurrencyName = currency;
        model.Info.Subtotal = "<span class='basket-subtotal-price'>" + subtotal.ToString("N2") + "</span> " + currency;

        var quote = await _shipping.CalculateAsync(new ShippingCalculateRequest
        {
            CartItems = model.CartList.Select(x => new ShippingCartItemRequest
            {
                ProductId = x.ProductID,
                Quantity = x.CartQuantity is < 1 ? 1 : x.CartQuantity!.Value
            }).ToList()
        }, cancellationToken);

        if (quote == null)
        {
            model.Info.GrandTotal = model.Info.Subtotal;
            model.Info.Total = model.Info.Subtotal;
            return;
        }

        model.Info.ShippingPrice = quote.ShippingPrice;
        model.Info.CarrierName = quote.CarrierName;
        model.Info.TotalDesi = quote.TotalDesi;
        model.Info.CarrierId = quote.CarrierId;

        var grandTotal = subtotal + quote.ShippingPrice;
        model.Info.GrandTotal = "<span class='basket-total-price'>" + grandTotal.ToString("N2") + "</span> " + currency;
        model.Info.Total = model.Info.GrandTotal;
    }

    private async Task<IActionResult> BasketJsonAsync(string userGuid, string languageCode, CancellationToken cancellationToken)
    {
        var routeResponse = await _basket.GetBasketAllAsync(new BasketAllRequest { UserID = userGuid, LanguageCode = languageCode }, cancellationToken);
        var model = CartBasketMergeHelper.BuildBasketSetModel(routeResponse.Status ? routeResponse.Data : null);
        await ApplyShippingQuoteAsync(model, cancellationToken);

        return Json(new
        {
            CartList = model.CartList.Select(c => new
            {
                productID = c.ProductID,
                name = c.ProductName,
                price = c.Price,
                currency = c.CurrencyName,
                currencyName = c.CurrencyName,
                image = c.MainImage,
                url = c.Url,
                quantity = c.CartQuantity ?? 0
            }),
            Info = CartBasketMergeHelper.BuildReturnInfo(model)
        });
    }
}
