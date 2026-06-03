using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class FooterSeoV1ViewComponent : ViewComponent
{
    public IViewComponentResult Invoke() => View();
}
