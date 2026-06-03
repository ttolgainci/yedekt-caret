using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class FooterSendMailV1ViewComponent : ViewComponent
{
    public IViewComponentResult Invoke() => View();
}
