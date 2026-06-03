using MarbleWebProject.Models;
using MarbleWebProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents
{

    [ViewComponent]
    public class HomeCompanyInfoV1ViewComponent : ViewComponent
    {
        
        public IViewComponentResult Invoke() => View();
    }
}
