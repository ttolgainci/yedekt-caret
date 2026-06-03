using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class FooterInfoTopV2ViewComponent : ViewComponent
{
    public IViewComponentResult Invoke() => View();
}
