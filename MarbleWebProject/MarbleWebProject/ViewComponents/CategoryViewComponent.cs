using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using MarbleWebProject.Services.Cache;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class CategoryViewComponent : ViewComponent
{
    private readonly IMemoryCache _cache;
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;
    private readonly IGlobalCatalogCacheVersion _catalogVersion;
    private readonly IWebCatalogCache _webCatalogCache;

    public CategoryViewComponent(
        IMemoryCache cache,
        IStoreCatalogApi catalog,
        IStoreAuthService auth,
        IGlobalCatalogCacheVersion catalogVersion,
        IWebCatalogCache webCatalogCache)
    {
        _cache = cache;
        _catalog = catalog;
        _auth = auth;
        _catalogVersion = catalogVersion;
        _webCatalogCache = webCatalogCache;
    }

    public async Task<IViewComponentResult> InvokeAsync(List<CategoryListModel> models, CancellationToken cancellationToken = default)
    {
        var session = await _auth.GetSessionAsync(cancellationToken);
        var tenant = AppConfig.ProjectName;
        var languageCode = session.LanguageCode;
        var version = _catalogVersion.GetCurrent(tenant);
        var key = $"CategoryListHeader:{tenant}:{languageCode}:{version}";

        var cacheHelper = new CacheHelper(_cache);
        if (cacheHelper.IfCache(key))
        {
            var cached = cacheHelper.GetCache(key) as List<CategoryListModel>;
            return View(cached ?? new List<CategoryListModel>());
        }

        var distributed = await _webCatalogCache.TryGetHeaderCategoriesAsync(tenant, languageCode, version, cancellationToken);
        if (distributed != null)
        {
            cacheHelper.SetCache(key, distributed);
            return View(distributed);
        }

        var routeResponse = await _catalog.GetCategoriesAsync(
            new CategoryRequest { languageCode = languageCode }, cancellationToken);

        if (routeResponse.Status && routeResponse.Data != null)
        {
            cacheHelper.SetCache(key, routeResponse.Data);
            await _webCatalogCache.SetHeaderCategoriesAsync(tenant, languageCode, version, routeResponse.Data, cancellationToken);
            return View(routeResponse.Data);
        }

        cacheHelper.SetCache(key, new List<CategoryListModel>());
        return Content(string.Empty);
    }
}
