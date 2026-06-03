using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class FooterInfoTopV1ViewComponent : ViewComponent
{
    public IViewComponentResult Invoke() => View();
}
