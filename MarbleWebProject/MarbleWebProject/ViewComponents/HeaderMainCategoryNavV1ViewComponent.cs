using MarbleWebProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HeaderMainCategoryNavV1ViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(HeaderNavigationModel model) => View(model);
}
