using MarbleWebProject.Models;
using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.ViewComponents;

[ViewComponent]
public class HeaderCategoryNavV2ViewComponent : ViewComponent
{
    public IViewComponentResult Invoke(HeaderNavigationModel model) => View(model);
}
