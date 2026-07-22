using MarbleWebProject.Helper;
using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HomeManufacturersBannerV1ViewComponent : ViewComponent
{
    private readonly IMemoryCache _cache;
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;

    public HomeManufacturersBannerV1ViewComponent(IMemoryCache cache, IStoreCatalogApi catalog, IStoreAuthService auth)
    {
        _cache = cache;
        _catalog = catalog;
        _auth = auth;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var brands = await HomeManufacturersBannerHelper.LoadAsync(_cache, _catalog, _auth, cancellationToken);
        if (brands == null)
            return Content(string.Empty);

        return View("~/Views/Shared/_HomeBrandsCarousel.cshtml", brands);
    }
}

internal static class HomeManufacturersBannerHelper
{
    public static async Task<List<HomeBrandModel>?> LoadAsync(
        IMemoryCache cache,
        IStoreCatalogApi catalog,
        IStoreAuthService auth,
        CancellationToken cancellationToken)
    {
        var cacheHelper = new CacheHelper(cache);
        var key = "HomeBrands:" + AppConfig.Storefront.StoreAuth.CustomName + ":" + AppConfig.Storefront.StoreAuth.LanguageCode;

        List<HomeBrandModel> brands;
        if (!cacheHelper.IfCache(key))
        {
            var session = await auth.GetSessionAsync(cancellationToken);
            var response = await catalog.GetStorefrontBrandsAsync(session.LanguageCode, cancellationToken);
            brands = response.Status && response.Data != null
                ? response.Data.Where(b => !string.IsNullOrWhiteSpace(b.Picture)).ToList()
                : new List<HomeBrandModel>();
            cacheHelper.SetCache(key, brands);
        }
        else
        {
            brands = cacheHelper.GetCache(key) as List<HomeBrandModel> ?? new List<HomeBrandModel>();
        }

        return brands.Count == 0 ? null : brands;
    }
}
