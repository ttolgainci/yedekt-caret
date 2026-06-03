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

    public async Task<IActionResult> Index(int id, int catID, CancellationToken cancellationToken = default)
    {
        var route = Request.Path.Value;
        var routeList = route?.Split("/") ?? Array.Empty<string>();
        if (routeList.Length != 3)
            return NotFound();

        var session = await _auth.GetSessionAsync(cancellationToken);
        var contentRequest = new ProductDetailRequest { Language = session.LanguageCode, ID = id };
        var routeResponse = await _catalog.GetProductDetailAsync(contentRequest, cancellationToken);

        if (!routeResponse.Status)
            return NotFound();

        routeResponse.Data.ProductSimilarData = new ProductSimilarModel { CategoryID = catID, ProductID = id };
        routeResponse.Data.ProductDetails.Url = route;
        TempData["Title"] = routeResponse.Data.ProductDetails.MetaTitle;
        TempData["Keywords"] = routeResponse.Data.ProductDetails.MetaKeyword;
        TempData["Description"] = routeResponse.Data.ProductDetails.MetaDescription;
        return View(routeResponse.Data);
    }
}
