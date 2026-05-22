using MarbleWebProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents
{

    [ViewComponent]
    public class ProductInfoViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(ProductInfoViewModel model)
        {
            return Task.FromResult<IViewComponentResult>(View(model));
        }
    }
}
