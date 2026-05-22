using MarbleWebProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;

namespace MarbleWebProject.ViewComponents
{

    [ViewComponent]
    public class ProductAttributeDetailViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(List<ProductAttributeList> model)
        {
            return Task.FromResult<IViewComponentResult>(View(model));
        }
    }
}
