using MarbleWebProject.Services.Storefront;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HomePageLayoutViewComponent : ViewComponent
{
    private readonly IStorefrontRuntimeProvider _storefront;

    public HomePageLayoutViewComponent(IStorefrontRuntimeProvider storefront) => _storefront = storefront;

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var runtime = await _storefront.GetAsync(cancellationToken);
        return View(runtime.Home);
    }
}
