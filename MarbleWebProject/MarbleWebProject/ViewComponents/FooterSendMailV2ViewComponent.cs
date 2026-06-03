using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class FooterSendMailV2ViewComponent : ViewComponent
{
    public IViewComponentResult Invoke() => View();
}
