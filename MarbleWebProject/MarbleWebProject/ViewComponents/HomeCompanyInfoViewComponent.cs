using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents
{

    [ViewComponent]
    public class HomeCompanyInfoViewComponent : ViewComponent
    {
        
        public async Task<IViewComponentResult> InvokeAsync()
        {

            return View();
        }
    }
}
