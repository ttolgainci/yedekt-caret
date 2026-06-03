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
        var rtn = new BasketSetModel();
        var userGuid = HttpContext.Request.Cookies["UserIDForBasket"] ?? "";
        await _auth.GetSessionAsync(cancellationToken);
        var routeResponse = await _basket.GetBasketAllAsync(new BasketAllRequest { UserID = userGuid }, cancellationToken);

        if (routeResponse.Status && routeResponse.Data != null && routeResponse.Data.Count > 0)
        {
            foreach (var item in routeResponse.Data)
            {
                rtn.CartList.Add(new CartModel
                {
                    CartQuantity = item.quantity,
                    CurrencyName = item.Currency,
                    MainImage = item.Image,
                    Price = item.Price,
                    ProductID = item.ProductID,
                    ProductName = item.Name,
                    Url = item.Url
                });
            }
            var currency = routeResponse.Data.FirstOrDefault()?.Currency ?? "";
            var getTotal = routeResponse.Data.Sum(c => c.Price * c.quantity);
            rtn.Info.Total = "<span class='basket-total-price'>" + getTotal + "</span>" + currency;
            rtn.Info.TotalQuantity = routeResponse.Data.Sum(c => c.quantity);
        }

        return View(rtn);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int ProductID, string Url, int? CartQuantity, CancellationToken cancellationToken = default)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var userGuid = HttpContext.Request.Cookies["UserIDForBasket"] ?? "";
        var contentRequest = new BasketRequest
        {
            LanguageCode = session.LanguageCode,
            ProductID = ProductID,
            CartQuantity = CartQuantity,
            Url = Url,
            UserID = userGuid
        };
        var routeResponse = await _basket.GetByIdBasketAsync(contentRequest, cancellationToken);
        var basketReturnModel = new BasketReturnModel();

        if (routeResponse.Status && routeResponse.Data != null && routeResponse.Data.Count > 0)
        {
            var getTotal = routeResponse.Data.Sum(c => c.Price * c.quantity);
            var currency = routeResponse.Data.FirstOrDefault()?.Currency ?? "";
            basketReturnModel.TotalPrice = "<span class='basket-total-price'>" + getTotal + "</span>" + currency;
            basketReturnModel.TotalQuantity = routeResponse.Data.Sum(c => c.quantity);
        }

        return Json(new { CartList = routeResponse.Data, Info = basketReturnModel });
    }

    [HttpPost]
    public async Task<IActionResult> DeleteFromCart(int ProductID, CancellationToken cancellationToken = default)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var contentRequest = new DeleteBasketRequest
        {
            LanguageCode = session.LanguageCode,
            ProductID = ProductID
        };
        var routeResponse = await _basket.DeleteProductFromCartAsync(contentRequest, cancellationToken);
        var basketReturnModel = new BasketReturnModel();

        if (routeResponse.Status && routeResponse.Data != null && routeResponse.Data.Count > 0)
        {
            var getTotal = routeResponse.Data.Sum(c => c.Price * c.quantity);
            var currency = routeResponse.Data.FirstOrDefault()?.Currency ?? "";
            basketReturnModel.TotalPrice = "<span class='basket-total-price'>" + getTotal + "</span>" + currency;
            basketReturnModel.TotalQuantity = routeResponse.Data.Sum(c => c.quantity);
        }

        return Json(new { CartList = routeResponse.Data, Info = basketReturnModel });
    }
}
