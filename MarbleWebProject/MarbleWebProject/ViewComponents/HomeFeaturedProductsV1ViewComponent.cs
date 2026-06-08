using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MarbleWebProject.ViewComponents;

//[ViewComponent]
//public class HomeFeaturedProductsV1ViewComponent : ViewComponent
//{
//    private readonly IMemoryCache _cache;
//    private readonly IStoreCatalogApi _catalog;
//    private readonly IStoreAuthService _auth;

//    public HomeFeaturedProductsV1ViewComponent(IMemoryCache cache, IStoreCatalogApi catalog, IStoreAuthService auth)
//    {
//        _cache = cache;
//        _catalog = catalog;
//        _auth = auth;
//    }

//    public async Task<IViewComponentResult> InvokeAsync(List<CategoryListModel>? models = null, CancellationToken cancellationToken = default)
//    {
//        var cacheHelper = new CacheHelper(_cache);
//        var key = "CategoryListHeader" + AppConfig.CMSService.CustomName + AppConfig.CMSService.MarketCode + AppConfig.CMSService.LanguageCode;

//        if (!cacheHelper.IfCache(key))
//        {
//            var session = await _auth.GetSessionAsync(cancellationToken);
//            var routeResponse = await _catalog.GetCategoriesAsync(
//                new CategoryRequest { languageCode = session.LanguageCode }, cancellationToken);

//            if (routeResponse.Status)
//            {
//                cacheHelper.SetCache(key, routeResponse.Data);
//                return View(routeResponse.Data);
//            }
//            cacheHelper.SetCache(key, new List<CategoryListModel>());
//            return Content(string.Empty);
//        }

//        var returnData = cacheHelper.GetCache(key) as List<CategoryListModel>;
//        return View(returnData ?? new List<CategoryListModel>());
//    }
//}
[ViewComponent]
public class HomeFeaturedProductsV1ViewComponent : ViewComponent
{
    private readonly IStoreStorefrontApi _storefront;

    public HomeFeaturedProductsV1ViewComponent(IStoreStorefrontApi storefront) => _storefront = storefront;

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        List<ProductList> products = new();
        try
        {
            var response = await _storefront.GetTopSellingProductsAsync(12, cancellationToken);
            if (response.Status && response.Data != null)
                products = response.Data;
        }
        catch
        {
            // empty → section hidden
        }

        return View(products);
    }
}