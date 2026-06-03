using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class FooterSeoV2ViewComponent : ViewComponent
{
    public IViewComponentResult Invoke() => View();
}
