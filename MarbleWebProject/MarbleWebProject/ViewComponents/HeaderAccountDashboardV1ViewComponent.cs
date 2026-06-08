using MarbleWebProject.Helpers;
using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HeaderAccountDashboardV1ViewComponent : ViewComponent
{
    private readonly IStoreCustomerSession _customerSession;

    public HeaderAccountDashboardV1ViewComponent(IStoreCustomerSession customerSession)
    {
        _customerSession = customerSession;
    }

    public async Task<IViewComponentResult> InvokeAsync(CancellationToken cancellationToken = default)
    {
        var model = await HeaderAccountMenuHelper.BuildAsync(_customerSession, cancellationToken);
        return View(model);
    }
}
