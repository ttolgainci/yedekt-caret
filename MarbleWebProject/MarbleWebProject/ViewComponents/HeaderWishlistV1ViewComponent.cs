using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HeaderWishlistV1ViewComponent : ViewComponent
{
    private readonly IStoreWishlistApi _wishlist;
    private readonly IStoreAuthService _auth;
    private readonly IBasketUserIdProvider _basketUserId;

    public HeaderWishlistV1ViewComponent(
        IStoreWishlistApi wishlist,
        IStoreAuthService auth,
        IBasketUserIdProvider basketUserId)
    {
        _wishlist = wishlist;
        _auth = auth;
        _basketUserId = basketUserId;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var model = new WishlistSetModel();
        var userId = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            var session = await _auth.GetSessionAsync(cancellationToken);
            var response = await _wishlist.GetAllAsync(new WishlistAllRequest
            {
                UserID = userId,
                LanguageCode = session.LanguageCode
            }, cancellationToken);

            if (response.Status && response.Data != null)
                model = WishlistHelper.BuildSetModelFromApiItems(response.Data);
        }

        return View(model);
    }
}
