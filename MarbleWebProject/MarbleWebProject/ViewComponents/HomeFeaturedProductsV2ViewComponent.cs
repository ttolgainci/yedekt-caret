using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HomeFeaturedProductsV2ViewComponent : ViewComponent
{
    private readonly IMemoryCache _cache;
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;

    public HomeFeaturedProductsV2ViewComponent(IMemoryCache cache, IStoreCatalogApi catalog, IStoreAuthService auth)
    {
        _cache = cache;
        _catalog = catalog;
        _auth = auth;
    }

    public async Task<IViewComponentResult> InvokeAsync(List<CategoryListModel>? models = null, CancellationToken cancellationToken = default)
    {
        var cacheHelper = new CacheHelper(_cache);
        var key = "CategoryListHeader" + AppConfig.CMSService.CustomName + AppConfig.CMSService.MarketCode + AppConfig.CMSService.LanguageCode;

        if (!cacheHelper.IfCache(key))
        {
            var session = await _auth.GetSessionAsync(cancellationToken);
            var routeResponse = await _catalog.GetCategoriesAsync(
                new CategoryRequest { languageCode = session.LanguageCode }, cancellationToken);

            if (routeResponse.Status)
            {
                cacheHelper.SetCache(key, routeResponse.Data);
                return View(routeResponse.Data);
            }
            cacheHelper.SetCache(key, new List<CategoryListModel>());
            return Content(string.Empty);
        }

        var returnData = cacheHelper.GetCache(key) as List<CategoryListModel>;
        return View(returnData ?? new List<CategoryListModel>());
    }
}
