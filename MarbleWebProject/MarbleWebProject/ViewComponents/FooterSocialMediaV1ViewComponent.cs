using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class FooterSocialMediaV1ViewComponent : ViewComponent
{
    public IViewComponentResult Invoke() => View();
}
