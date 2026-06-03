using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class ProductBreadcrumbViewComponent : ViewComponent
{
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;

    public ProductBreadcrumbViewComponent(IStoreCatalogApi catalog, IStoreAuthService auth)
    {
        _catalog = catalog;
        _auth = auth;
    }

    public async Task<IViewComponentResult> InvokeAsync(string name, CancellationToken cancellationToken = default)
    {
        var route = Request.Path.Value;
        var routeList = route?.Split('/') ?? Array.Empty<string>();
        if (routeList.Length < 2)
            return Content(string.Empty);

        var categoryUrl = routeList[^2];
        var session = await _auth.GetSessionAsync(cancellationToken);
        var routeResponse = await _catalog.GetProductBreadcrumbPathAsync(new ProductBreadcrumbRequest
        {
            LanguageCode = session.LanguageCode,
            CategoryUrl = categoryUrl
        }, cancellationToken);

        if (!routeResponse.Status)
            return Content(string.Empty);

        return View(new ProductBreadcrumbModel { CagetoryList = routeResponse.Data, Name = name });
    }
}
