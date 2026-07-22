using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class ProductDetailSimilarViewComponent : ViewComponent
{
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;

    public ProductDetailSimilarViewComponent(IStoreCatalogApi catalog, IStoreAuthService auth)
    {
        _catalog = catalog;
        _auth = auth;
    }

    public async Task<IViewComponentResult> InvokeAsync(ProductSimilarModel? data, CancellationToken cancellationToken = default)
    {
        if (data == null || data.ProductID <= 0 || data.CategoryID <= 0)
            return Content(string.Empty);

        var session = await _auth.GetSessionAsync(cancellationToken);
        var routeResponse = await _catalog.GetProductDetailSimilarAsync(new ProductsByCageoryRequest
        {
            LanguageCode = session.LanguageCode,
            ID = data.ProductID,
            CategoryID = data.CategoryID
        }, cancellationToken);

        if (routeResponse.Status && routeResponse.Data != null && routeResponse.Data.Count > 0)
            return View(routeResponse.Data);

        return Content(string.Empty);
    }
}
