using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HomeDiscountedProductsV1ViewComponent : ViewComponent
{
    private readonly IStoreStorefrontApi _storefront;

    public HomeDiscountedProductsV1ViewComponent(IStoreStorefrontApi storefront) => _storefront = storefront;

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        List<ProductList> products = new();
        try
        {
            var response = await _storefront.GetDiscountedProductsAsync(12, cancellationToken);
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
