using MarbleWebProject.Helpers;
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

    public IActionResult Index(int id)
    {
        var slug = Request.Path.Value?.Trim('/').Split('/').LastOrDefault();
        if (!string.IsNullOrWhiteSpace(slug) && id > 0)
            return RedirectPermanent(UrlSlugHelper.BuildCategoryPath(slug, id));

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

        if (routeResponse.Status && routeResponse.Data != null)
        {
            var slug = UrlSlugHelper.NormalizeSlug(routeResponse.Data.Url);
            if (!string.IsNullOrEmpty(slug))
            {
                var url = UrlSlugHelper.BuildCategoryPath(slug, routeResponse.Data.CategoryID);
                if (pageNumber > 1)
                    url += $"?pageNumber={pageNumber}";
                return RedirectPermanent(url);
            }
        }

        return NotFound();
    }
}
