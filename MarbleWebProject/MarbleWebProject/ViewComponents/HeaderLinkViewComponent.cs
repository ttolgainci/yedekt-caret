using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents
{

    [ViewComponent]
    public class HeaderLinkViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
