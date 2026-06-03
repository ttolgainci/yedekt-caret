using MarbleWebProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents
{

    [ViewComponent]
    public class ProductDetailImageViewComponent : ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync(List<ProductImageList> model)
        {
            return View(model);
        }
    }
}
