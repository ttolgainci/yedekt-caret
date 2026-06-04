using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HomeManufacturersBannerV2ViewComponent : ViewComponent
{
    private readonly IMemoryCache _cache;
    private readonly IStoreCatalogApi _catalog;
    private readonly IStoreAuthService _auth;

    public HomeManufacturersBannerV2ViewComponent(IMemoryCache cache, IStoreCatalogApi catalog, IStoreAuthService auth)
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
