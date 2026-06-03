using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HeaderSignInV2ViewComponent : ViewComponent
{
    public Task<IViewComponentResult> InvokeAsync() => Task.FromResult<IViewComponentResult>(View());
}
