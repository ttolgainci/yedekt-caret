using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers;

public class WishlistController : Controller
{
    private readonly IStoreWishlistApi _wishlist;
    private readonly IStoreAuthService _auth;
    private readonly IStoreCustomerSession _customerSession;

    public WishlistController(
        IStoreWishlistApi wishlist,
        IStoreAuthService auth,
        IStoreCustomerSession customerSession)
    {
        _wishlist = wishlist;
        _auth = auth;
        _customerSession = customerSession;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken = default)
    {
        if (!await IsMemberAsync(cancellationToken))
        {
            ViewBag.WishlistRequiresLogin = true;
            return View(new WishlistSetModel());
        }

        var model = await LoadWishlistSetAsync(cancellationToken);
        return View(model);
    }

    [HttpGet]
    [Route("Wishlist/Snapshot")]
    public async Task<IActionResult> Snapshot(CancellationToken cancellationToken = default)
    {
        if (!await IsMemberAsync(cancellationToken))
        {
            return Json(new
            {
                requiresLogin = true,
                totalCount = 0,
                productIds = Array.Empty<int>(),
                items = Array.Empty<object>()
            });
        }

        var customerId = await ResolveMemberUserIdAsync(cancellationToken);
        var session = await _auth.GetSessionAsync(cancellationToken);
        var languageCode = RequireLanguage(session.LanguageCode);
        var response = await _wishlist.GetAllAsync(new WishlistAllRequest
        {
            UserID = customerId,
            LanguageCode = languageCode
        }, cancellationToken);

        var items = response.Status && response.Data != null ? response.Data : new List<WishlistApiItem>();
        var lowStockAlert = StoreLowStockAlert.BuildPayload(
            items.Select(x => (
                x.ProductID,
                x.Name ?? string.Empty,
                MediaUrlHelper.BuildProductImage(x.Image),
                x.Url ?? "#",
                x.StockQuantity)),
            "/wishlist");

        return Json(new
        {
            requiresLogin = false,
            totalCount = items.Count,
            productIds = items.Select(x => x.ProductID).ToList(),
            items = items.Select(x => new
            {
                productID = x.ProductID,
                name = x.Name,
                price = x.Price,
                currency = x.Currency,
                image = MediaUrlHelper.BuildProductImage(x.Image),
                url = x.Url,
                stockQuantity = x.StockQuantity
            }),
            lowStockAlert
        });
    }

    [HttpPost]
    [Route("Wishlist/Toggle")]
    public async Task<IActionResult> Toggle(int productId, string? url, CancellationToken cancellationToken = default)
    {
        if (productId <= 0)
            return BadRequest(new { message = "Geçersiz ürün." });

        if (!await IsMemberAsync(cancellationToken))
            return Unauthorized(new { requiresLogin = true, message = "Favorilere eklemek için giriş yapmalısınız." });

        var customerId = await ResolveMemberUserIdAsync(cancellationToken);
        var session = await _auth.GetSessionAsync(cancellationToken);
        var languageCode = RequireLanguage(session.LanguageCode);
        var response = await _wishlist.ToggleAsync(new WishlistToggleRequest
        {
            ProductID = productId,
            UserID = customerId,
            LanguageCode = languageCode,
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

        if (!await IsMemberAsync(cancellationToken))
            return Unauthorized(new { requiresLogin = true, message = "Giriş yapmalısınız." });

        var customerId = await ResolveMemberUserIdAsync(cancellationToken);
        var session = await _auth.GetSessionAsync(cancellationToken);
        var languageCode = RequireLanguage(session.LanguageCode);
        var response = await _wishlist.RemoveAsync(new WishlistRemoveRequest
        {
            ProductID = productId,
            UserID = customerId,
            LanguageCode = languageCode
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
        var customerId = await ResolveMemberUserIdAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(customerId))
            return new WishlistSetModel();

        var session = await _auth.GetSessionAsync(cancellationToken);
        var response = await _wishlist.GetAllAsync(new WishlistAllRequest
        {
            UserID = customerId,
            LanguageCode = RequireLanguage(session.LanguageCode)
        }, cancellationToken);

        return response.Status && response.Data != null
            ? WishlistHelper.BuildSetModelFromApiItems(response.Data)
            : new WishlistSetModel();
    }

    private async Task<bool> IsMemberAsync(CancellationToken cancellationToken)
    {
        var auth = await _customerSession.GetAsync(cancellationToken);
        return auth?.Customer?.Id is > 0;
    }

    private async Task<string> ResolveMemberUserIdAsync(CancellationToken cancellationToken)
    {
        var auth = await _customerSession.GetAsync(cancellationToken);
        return auth?.Customer?.Id is > 0 ? auth.Customer.Id.ToString() : string.Empty;
    }

    private static string RequireLanguage(string? languageCode) =>
        string.IsNullOrWhiteSpace(languageCode) ? "tr" : languageCode.Trim();
}
