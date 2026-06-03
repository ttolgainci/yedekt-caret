using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class FooterLinkV1ViewComponent : ViewComponent
{
    public IViewComponentResult Invoke() => View();
}
