using MarbleWebProject.Services.Api;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HeaderSignInV2ViewComponent : ViewComponent
{
    private readonly IStoreCustomerSession _customerSession;

    public HeaderSignInV2ViewComponent(IStoreCustomerSession customerSession)
    {
        _customerSession = customerSession;
    }

    public Task<IViewComponentResult> InvokeAsync()
    {
        if (_customerSession.IsLoggedIn())
            return Task.FromResult<IViewComponentResult>(Content(string.Empty));

        return Task.FromResult<IViewComponentResult>(View());
    }
}
