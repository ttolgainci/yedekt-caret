using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HeaderBasketV1ViewComponent : ViewComponent
{
    private readonly IStoreBasketApi _basket;
    private readonly IStoreAuthService _auth;

    public HeaderBasketV1ViewComponent(IStoreBasketApi basket, IStoreAuthService auth)
    {
        _basket = basket;
        _auth = auth;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
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
}
