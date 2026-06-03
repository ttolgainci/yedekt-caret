using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents
{

    [ViewComponent]
    public class HomeInstagramV1ViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke() => View();
    }
}
