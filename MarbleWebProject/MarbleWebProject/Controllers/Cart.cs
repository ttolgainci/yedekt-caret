using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers;

public class Cart : Controller
{
    private readonly IStoreBasketApi _basket;
    private readonly IStoreAuthService _auth;

    public Cart(IStoreBasketApi basket, IStoreAuthService auth)
    {
        _basket = basket;
        _auth = auth;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var userGuid = GetBasketUserId();
        await _auth.GetSessionAsync(cancellationToken);
        var model = await LoadBasketSetModelAsync(userGuid, cancellationToken);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int ProductID, string Url, int? CartQuantity, CancellationToken cancellationToken = default)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var userGuid = GetBasketUserId();
        var qty = CartQuantity is < 1 ? 1 : CartQuantity!.Value;

        await _basket.GetByIdBasketAsync(new BasketRequest
        {
            LanguageCode = session.LanguageCode,
            ProductID = ProductID,
            CartQuantity = qty,
            Url = Url ?? string.Empty,
            UserID = userGuid
        }, cancellationToken);

        return await BasketJsonAsync(userGuid, cancellationToken);
    }

    [HttpPost]
    public async Task<IActionResult> ChangeCartQuantity(int ProductID, string Url, int Delta, CancellationToken cancellationToken = default)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var userGuid = GetBasketUserId();
        var all = await _basket.GetBasketAllAsync(new BasketAllRequest { UserID = userGuid }, cancellationToken);
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

        return await BasketJsonAsync(userGuid, cancellationToken);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteFromCart(int ProductID, CancellationToken cancellationToken = default)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var userGuid = GetBasketUserId();

        await _basket.DeleteProductFromCartAsync(new DeleteBasketRequest
        {
            ProductID = ProductID,
            UserID = userGuid,
            LanguageCode = session.LanguageCode
        }, cancellationToken);

        return await BasketJsonAsync(userGuid, cancellationToken);
    }

    private string GetBasketUserId() =>
        HttpContext.Request.Cookies["UserIDForBasket"] ?? string.Empty;

    private async Task<BasketSetModel> LoadBasketSetModelAsync(string userGuid, CancellationToken cancellationToken)
    {
        var routeResponse = await _basket.GetBasketAllAsync(new BasketAllRequest { UserID = userGuid }, cancellationToken);
        return routeResponse.Status && routeResponse.Data != null
            ? CartBasketMergeHelper.BuildBasketSetModel(routeResponse.Data)
            : new BasketSetModel();
    }

    private async Task<IActionResult> BasketJsonAsync(string userGuid, CancellationToken cancellationToken)
    {
        var routeResponse = await _basket.GetBasketAllAsync(new BasketAllRequest { UserID = userGuid }, cancellationToken);
        var merged = routeResponse.Status && routeResponse.Data != null
            ? CartBasketMergeHelper.MergeLines(routeResponse.Data)
            : new List<OrderBasket>();

        return Json(new
        {
            CartList = merged,
            Info = CartBasketMergeHelper.BuildReturnInfo(merged)
        });
    }
}
