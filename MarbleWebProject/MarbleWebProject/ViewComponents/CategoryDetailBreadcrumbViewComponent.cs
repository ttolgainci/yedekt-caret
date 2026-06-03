using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class CategoryDetailBreadcrumbViewComponent : ViewComponent
{
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;

    public CategoryDetailBreadcrumbViewComponent(IStoreCatalogApi catalog, IStoreAuthService auth)
    {
        _catalog = catalog;
        _auth = auth;
    }

    public async Task<IViewComponentResult> InvokeAsync(int categoryID, CancellationToken cancellationToken = default)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var routeResponse = await _catalog.GetCategoryBreadcrumbAsync(new CategoryBreadcrumbRequest
        {
            LanguageCode = session.LanguageCode,
            CategoryID = categoryID
        }, cancellationToken);

        if (routeResponse.Status)
            return View(routeResponse.Data);

        return Content(string.Empty);
    }
}
