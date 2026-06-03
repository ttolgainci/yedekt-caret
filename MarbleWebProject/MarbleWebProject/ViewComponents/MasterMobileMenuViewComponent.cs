using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class MasterMobileMenuViewComponent : ViewComponent
{
    private readonly IMemoryCache _cache;
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;

    public MasterMobileMenuViewComponent(IMemoryCache cache, IStoreCatalogApi catalog, IStoreAuthService auth)
    {
        _cache = cache;
        _catalog = catalog;
        _auth = auth;
    }

    public async Task<IViewComponentResult> InvokeAsync(MobileMenuModel models, CancellationToken cancellationToken = default)
    {
        var allModel = new MobileMenuModel();
        var cacheHelper = new CacheHelper(_cache);
        var key = "CategoryMobileListHeader" + AppConfig.CMSService.CustomName + AppConfig.CMSService.MarketCode + AppConfig.CMSService.LanguageCode;
        List<CategoryListModel> returnData;

        if (!cacheHelper.IfCache(key))
        {
            var session = await _auth.GetSessionAsync(cancellationToken);
            var routeResponse = await _catalog.GetCategoriesAsync(
                new CategoryRequest { languageCode = session.LanguageCode }, cancellationToken);

            if (!routeResponse.Status)
            {
                cacheHelper.SetCache(key, new List<CategoryListModel>());
                return Content(string.Empty);
            }

            returnData = routeResponse.Data;
            cacheHelper.SetCache(key, returnData);
        }
        else
        {
            returnData = cacheHelper.GetCache(key) as List<CategoryListModel> ?? new List<CategoryListModel>();
        }

        allModel.Menu = returnData;
        allModel.HeaderLink = new List<HeaderLink>();
        return View(allModel);
    }
}
