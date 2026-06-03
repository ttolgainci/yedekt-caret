using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents
{
    [ViewComponent]
    public class FooterLinkV2ViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke() => View();
    }
}
