using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents
{


    [ViewComponent]
    public class LoginRegisterViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
