using MarbleWebProject.Models;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HomeDealOfDayV2ViewComponent : ViewComponent
{
    private readonly IStoreStorefrontApi _storefront;

    public HomeDealOfDayV2ViewComponent(IStoreStorefrontApi storefront) => _storefront = storefront;

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        CampaignActiveModel? campaign = null;
        try
        {
            var response = await _storefront.GetActiveCampaignsAsync(cancellationToken);
            if (response.Status && response.Data?.Count > 0)
                campaign = response.Data[0];
        }
        catch
        {
            // static fallback in view
        }

        return View(campaign);
    }
}
