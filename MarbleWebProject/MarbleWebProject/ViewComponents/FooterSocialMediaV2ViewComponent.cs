using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class FooterSocialMediaV2ViewComponent : ViewComponent
{
    public IViewComponentResult Invoke() => View();
}
