using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers;

public class ProductDetailController : Controller
{
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;

    public ProductDetailController(IStoreCatalogApi catalog, IStoreAuthService auth)
    {
        _catalog = catalog;
        _auth = auth;
    }

    /// <summary>Canonical product page: /{brand}/{slug}-p-{id}</summary>
    public async Task<IActionResult> Index(
        string? brandSlug,
        string? productSegment,
        int? id,
        int catID = 0,
        CancellationToken cancellationToken = default)
    {
        var path = Request.Path.Value ?? "";
        var productId = 0;
        if (id is > 0)
            productId = id.Value;
        else if (!UrlSlugHelper.TryParseProductId(productSegment ?? path, out productId))
            return NotFound();

        var session = await _auth.GetSessionAsync(cancellationToken);
        var canonicalResp = await _catalog.GetProductCanonicalUrlAsync(productId, session.LanguageCode, cancellationToken);
        if (!canonicalResp.Status || string.IsNullOrWhiteSpace(canonicalResp.Data))
            return NotFound();

        var canonicalPath = canonicalResp.Data!;
        if (!canonicalPath.StartsWith('/'))
            canonicalPath = "/" + canonicalPath;

        if (!UrlSlugHelper.PathsEqual(path, canonicalPath))
            return RedirectPermanent(canonicalPath);

        var contentRequest = new ProductDetailRequest { Language = session.LanguageCode, ID = productId };
        var routeResponse = await _catalog.GetProductDetailAsync(contentRequest, cancellationToken);
        if (!routeResponse.Status || routeResponse.Data?.ProductDetails == null)
            return NotFound();

        routeResponse.Data.ProductSimilarData = new ProductSimilarModel
        {
            CategoryID = catID > 0 ? catID : routeResponse.Data.ProductDetails.CategoryID,
            ProductID = productId
        };
        routeResponse.Data.ProductDetails.Url = canonicalPath;
        TempData["Title"] = routeResponse.Data.ProductDetails.MetaTitle;
        TempData["Keywords"] = routeResponse.Data.ProductDetails.MetaKeyword;
        TempData["Description"] = routeResponse.Data.ProductDetails.MetaDescription;
        ViewData["CanonicalUrl"] = canonicalPath;
        ViewData["ProductId"] = productId;

        var alts = await _catalog.GetProductAlternateUrlsAsync(productId, cancellationToken);
        if (alts.Status && alts.Data != null)
            ViewData["HreflangAlternates"] = alts.Data;

        return View(routeResponse.Data);
    }

    /// <summary>Legacy /{category}/{product} routes — permanent redirect to canonical.</summary>
    public async Task<IActionResult> RedirectCanonical(int id, CancellationToken cancellationToken = default)
    {
        if (id <= 0)
            return NotFound();

        var session = await _auth.GetSessionAsync(cancellationToken);
        var canonicalResp = await _catalog.GetProductCanonicalUrlAsync(id, session.LanguageCode, cancellationToken);
        if (!canonicalResp.Status || string.IsNullOrWhiteSpace(canonicalResp.Data))
            return NotFound();

        var canonicalPath = canonicalResp.Data!;
        if (!canonicalPath.StartsWith('/'))
            canonicalPath = "/" + canonicalPath;

        return RedirectPermanent(canonicalPath);
    }
}
