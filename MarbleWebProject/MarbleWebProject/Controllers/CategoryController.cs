using MarbleWebProject.Models;
using MarbleWebProject.Services;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers;

public class CategoryController : Controller
{
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;

    public CategoryController(IStoreCatalogApi catalog, IStoreAuthService auth)
    {
        _catalog = catalog;
        _auth = auth;
    }

    public async Task<IActionResult> Index(int id, int pageNumber = 1, int pageSize = 12, CancellationToken cancellationToken = default)
    {
        var route = Request.Path.Value;
        var session = await _auth.GetSessionAsync(cancellationToken);
        var contentRequest = new ProductsByCageoryRequest
        {
            pageNumber = pageNumber,
            pageSize = pageSize,
            LanguageCode = session.LanguageCode,
            ID = id
        };
        var routeResponse = await _catalog.GetProductsByCategoryAsync(contentRequest, cancellationToken);

        if (routeResponse.Status)
        {
            TempData["catUrl"] = route?.TrimEnd('/').Split('/').Last();
            TempData["catId"] = id;
            TempData["Title"] = routeResponse.Data.MetaTitle;
            TempData["Keywords"] = routeResponse.Data.MetaKeyword;
            TempData["Description"] = routeResponse.Data.MetaDesc;
            return View(routeResponse.Data);
        }

        return RedirectToAction("PageNotFound", "Error");
    }

    public async Task<IActionResult> GetProducts(int id, int pageNumber = 1, int pageSize = 12, CancellationToken cancellationToken = default)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var contentRequest = new ProductsByCageoryRequest
        {
            pageNumber = pageNumber,
            pageSize = pageSize,
            LanguageCode = session.LanguageCode,
            ID = id
        };
        var routeResponse = await _catalog.GetProductsByCategoryAsync(contentRequest, cancellationToken);

        if (routeResponse.Status)
        {
            TempData["catId"] = id;
            TempData["Title"] = routeResponse.Data.MetaTitle;
            TempData["Keywords"] = routeResponse.Data.MetaKeyword;
            TempData["Description"] = routeResponse.Data.MetaDesc;
            return PartialView("_CategoryProductListPartial", routeResponse.Data);
        }

        return NotFound();
    }
}
