using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents
{

    [ViewComponent]
    public class HeaderSearchViewComponent : ViewComponent
    {      
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
