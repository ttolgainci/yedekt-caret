using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers;

public class WishlistController : Controller
{
    private readonly IStoreWishlistApi _wishlist;
    private readonly IStoreAuthService _auth;
    private readonly IBasketUserIdProvider _basketUserId;

    public WishlistController(
        IStoreWishlistApi wishlist,
        IStoreAuthService auth,
        IBasketUserIdProvider basketUserId)
    {
        _wishlist = wishlist;
        _auth = auth;
        _basketUserId = basketUserId;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        var model = await LoadWishlistSetAsync(cancellationToken);
        return View(model);
    }

    [HttpGet]
    [Route("Wishlist/Snapshot")]
    public async Task<IActionResult> Snapshot(CancellationToken cancellationToken = default)
    {
        var userId = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Json(new
            {
                totalCount = 0,
                productIds = Array.Empty<int>(),
                items = Array.Empty<object>()
            });
        }

        var session = await _auth.GetSessionAsync(cancellationToken);
        var response = await _wishlist.GetAllAsync(new WishlistAllRequest
        {
            UserID = userId,
            LanguageCode = session.LanguageCode
        }, cancellationToken);

        var items = response.Status && response.Data != null ? response.Data : new List<WishlistApiItem>();
        return Json(new
        {
            totalCount = items.Count,
            productIds = items.Select(x => x.ProductID).ToList(),
            items = items.Select(x => new
            {
                productID = x.ProductID,
                name = x.Name,
                price = x.Price,
                currency = x.Currency,
                image = MediaUrlHelper.BuildProductImage(x.Image),
                url = x.Url
            })
        });
    }

    [HttpPost]
    [Route("Wishlist/Toggle")]
    public async Task<IActionResult> Toggle(int productId, string? url, CancellationToken cancellationToken = default)
    {
        if (productId <= 0)
            return BadRequest(new { message = "Geçersiz ürün." });

        var userId = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(userId))
            return BadRequest(new { message = "Oturum kimliği bulunamadı." });

        var session = await _auth.GetSessionAsync(cancellationToken);
        var response = await _wishlist.ToggleAsync(new WishlistToggleRequest
        {
            ProductID = productId,
            UserID = userId,
            LanguageCode = session.LanguageCode,
            Url = url ?? string.Empty
        }, cancellationToken);

        if (!response.Status || response.Data == null)
            return BadRequest(new { message = response.ErrorMessage ?? "İşlem başarısız." });

        return Json(new
        {
            isInWishlist = response.Data.IsInWishlist,
            totalCount = response.Data.TotalCount,
            productIds = response.Data.Items.Select(x => x.ProductID).ToList(),
            items = response.Data.Items.Select(x => new
            {
                productID = x.ProductID,
                name = x.Name,
                price = x.Price,
                currency = x.Currency,
                image = MediaUrlHelper.BuildProductImage(x.Image),
                url = x.Url
            })
        });
    }

    [HttpPost]
    [Route("Wishlist/Remove")]
    public async Task<IActionResult> Remove(int productId, CancellationToken cancellationToken = default)
    {
        if (productId <= 0)
            return BadRequest(new { message = "Geçersiz ürün." });

        var userId = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        var session = await _auth.GetSessionAsync(cancellationToken);
        var response = await _wishlist.RemoveAsync(new WishlistRemoveRequest
        {
            ProductID = productId,
            UserID = userId,
            LanguageCode = session.LanguageCode
        }, cancellationToken);

        var items = response.Status && response.Data != null ? response.Data : new List<WishlistApiItem>();
        return Json(new
        {
            totalCount = items.Count,
            productIds = items.Select(x => x.ProductID).ToList()
        });
    }

    private async Task<WishlistSetModel> LoadWishlistSetAsync(CancellationToken cancellationToken)
    {
        var userId = await _basketUserId.ResolveBasketUserIdAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(userId))
            return new WishlistSetModel();

        var session = await _auth.GetSessionAsync(cancellationToken);
        var response = await _wishlist.GetAllAsync(new WishlistAllRequest
        {
            UserID = userId,
            LanguageCode = session.LanguageCode
        }, cancellationToken);

        return response.Status && response.Data != null
            ? WishlistHelper.BuildSetModelFromApiItems(response.Data)
            : new WishlistSetModel();
    }
}
