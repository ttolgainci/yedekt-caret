using MarbleWebProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents
{
    [ViewComponent]
    public class ProductInfoViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(ProductInfoPageModel model)
        {
            model ??= new ProductInfoPageModel();
            model.Product ??= new ProductInfo();
            return Task.FromResult<IViewComponentResult>(View(model));
        }
    }
}
