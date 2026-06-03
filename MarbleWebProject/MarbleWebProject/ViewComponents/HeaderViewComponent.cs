using MarbleWebProject.Helper;
using MarbleWebProject.Helpers;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HeaderViewComponent : ViewComponent
{
    private readonly IMemoryCache _cache;
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;
    private readonly ProjectServiceSettings _project;

    public HeaderViewComponent(
        IMemoryCache cache,
        IStoreCatalogApi catalog,
        IStoreAuthService auth,
        IOptions<ProjectServiceSettings> project)
    {
        _cache = cache;
        _catalog = catalog;
        _auth = auth;
        _project = project.Value;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var categories = await GetCategoriesAsync(cancellationToken);
        var nav = CategoryNavHelper.BuildNavigation(categories, _project.HeaderNavMaxVisibleCategories);
        return View(nav);
    }

    private async Task<List<CategoryListModel>> GetCategoriesAsync(CancellationToken cancellationToken)
    {
        var cacheHelper = new CacheHelper(_cache);
        var key = "CategoryListHeader" + AppConfig.CMSService.CustomName
            + AppConfig.CMSService.MarketCode + AppConfig.CMSService.LanguageCode;

        if (cacheHelper.IfCache(key))
            return cacheHelper.GetCache(key) as List<CategoryListModel> ?? new List<CategoryListModel>();

        var session = await _auth.GetSessionAsync(cancellationToken);
        var response = await _catalog.GetCategoriesAsync(
            new CategoryRequest { languageCode = session.LanguageCode }, cancellationToken);

        var list = response.Status ? response.Data ?? new List<CategoryListModel>() : new List<CategoryListModel>();
        cacheHelper.SetCache(key, list);
        return list;
    }
}
