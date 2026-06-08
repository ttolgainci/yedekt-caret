using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

//[ViewComponent]
//public class HomeTopSellingV1ViewComponent : ViewComponent
//{
//    private readonly IStoreStorefrontApi _storefront;

//    public HomeTopSellingV1ViewComponent(IStoreStorefrontApi storefront) => _storefront = storefront;

//    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
//    {
//        List<ProductList> products = new();
//        try
//        {
//            var response = await _storefront.GetHomeListedProductsAsync(8, cancellationToken);
//            if (response.Status && response.Data != null)
//                products = response.Data;
//        }
//        catch
//        {
//            // empty → view shows theme placeholder or hides carousel items
//        }

//        return View(products);
//    }
//}
[ViewComponent]
public class HomeTopSellingV1ViewComponent : ViewComponent
{
    private readonly IStoreStorefrontApi _storefront;

    public HomeTopSellingV1ViewComponent(IStoreStorefrontApi storefront) => _storefront = storefront;

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