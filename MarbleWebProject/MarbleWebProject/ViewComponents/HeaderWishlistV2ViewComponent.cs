using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HeaderWishlistV2ViewComponent : ViewComponent
{
    private readonly IStoreWishlistApi _wishlist;
    private readonly IStoreAuthService _auth;
    private readonly IStoreCustomerSession _customerSession;

    public HeaderWishlistV2ViewComponent(
        IStoreWishlistApi wishlist,
        IStoreAuthService auth,
        IStoreCustomerSession customerSession)
    {
        _wishlist = wishlist;
        _auth = auth;
        _customerSession = customerSession;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var model = new WishlistSetModel();
        var auth = await _customerSession.GetAsync(cancellationToken);
        if (auth?.Customer?.Id is > 0)
        {
            var session = await _auth.GetSessionAsync(cancellationToken);
            var languageCode = string.IsNullOrWhiteSpace(session.LanguageCode) ? "tr" : session.LanguageCode.Trim();
            var response = await _wishlist.GetAllAsync(new WishlistAllRequest
            {
                UserID = auth.Customer.Id.ToString(),
                LanguageCode = languageCode
            }, cancellationToken);

            if (response.Status && response.Data != null)
                model = WishlistHelper.BuildSetModelFromApiItems(response.Data);
        }

        return View(model);
    }
}
