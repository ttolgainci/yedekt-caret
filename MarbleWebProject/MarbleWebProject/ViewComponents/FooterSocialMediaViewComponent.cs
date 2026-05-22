using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents
{

    [ViewComponent]
    public class FooterSocialMediaViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
