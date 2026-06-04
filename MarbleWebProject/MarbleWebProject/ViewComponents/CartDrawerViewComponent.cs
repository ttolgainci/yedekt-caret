using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent(Name = "CartDrawer")]
public class CartDrawerViewComponent : ViewComponent
{
    private readonly IStoreBasketApi _basket;
    private readonly IStoreAuthService _auth;

    public CartDrawerViewComponent(IStoreBasketApi basket, IStoreAuthService auth)
    {
        _basket = basket;
        _auth = auth;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var userGuid = HttpContext.Request.Cookies["UserIDForBasket"] ?? "";
        await _auth.GetSessionAsync(cancellationToken);
        var routeResponse = await _basket.GetBasketAllAsync(new BasketAllRequest { UserID = userGuid }, cancellationToken);

        var model = routeResponse.Status && routeResponse.Data != null
            ? CartBasketMergeHelper.BuildBasketSetModel(routeResponse.Data)
            : new BasketSetModel();

        return View(model);
    }
}
