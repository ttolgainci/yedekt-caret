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

    /// <summary>
    /// Canonical URL /{brand}/{slug}-p-{id} olduğu için kategori path'ten değil CategoryID ile çözülür.
    /// </summary>
    public async Task<IViewComponentResult> InvokeAsync(string name, int categoryId = 0, CancellationToken cancellationToken = default)
    {
        var categories = new List<CategoryBreadcrumbModel>();

        if (categoryId > 0)
        {
            var session = await _auth.GetSessionAsync(cancellationToken);
            var routeResponse = await _catalog.GetCategoryBreadcrumbAsync(new CategoryBreadcrumbRequest
            {
                LanguageCode = session.LanguageCode,
                CategoryID = categoryId
            }, cancellationToken);

            if (routeResponse.Status && routeResponse.Data != null)
                categories = routeResponse.Data;
        }

        return View(new ProductBreadcrumbModel
        {
            CagetoryList = categories,
            Name = name ?? ""
        });
    }
}
