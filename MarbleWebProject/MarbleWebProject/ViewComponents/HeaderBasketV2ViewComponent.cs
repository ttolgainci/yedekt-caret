using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HeaderBasketV2ViewComponent : ViewComponent
{
    private readonly IStoreBasketApi _basket;
    private readonly IStoreAuthService _auth;
    private readonly IBasketUserIdProvider _basketUserId;

    public HeaderBasketV2ViewComponent(IStoreBasketApi basket, IStoreAuthService auth, IBasketUserIdProvider basketUserId)
    {
        _basket = basket;
        _auth = auth;
        _basketUserId = basketUserId;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var userGuid = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        var session = await _auth.GetSessionAsync(cancellationToken);
        var routeResponse = await _basket.GetBasketAllAsync(new BasketAllRequest { UserID = userGuid, LanguageCode = session.LanguageCode }, cancellationToken);

        var model = routeResponse.Status && routeResponse.Data != null
            ? CartBasketMergeHelper.BuildBasketSetModel(routeResponse.Data)
            : new BasketSetModel();

        return View(model);
    }
}
