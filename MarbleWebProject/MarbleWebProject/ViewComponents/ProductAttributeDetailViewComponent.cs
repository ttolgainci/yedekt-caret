using MarbleWebProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents
{
    [ViewComponent]
    public class ProductAttributeDetailViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(ProductDetailTabsViewModel model)
        {
            model ??= new ProductDetailTabsViewModel();
            model.Attributes ??= new List<ProductAttributeList>();
            return Task.FromResult<IViewComponentResult>(View(model));
        }
    }
}
