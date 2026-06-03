using MarbleWebProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace MarbleWebProject.ViewComponents
{

    [ViewComponent]
    public class ProductAttributeDetailViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(List<ProductAttributeList> model)
        {
            return View(model);
        }
    }
}
