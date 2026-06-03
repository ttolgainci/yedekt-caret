using MarbleWebProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents
{

    [ViewComponent]
    public class ProductInfoViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(ProductInfo model)
        {
            return View(model);
        }
    }
}
