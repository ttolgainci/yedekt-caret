using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers
{
    public class ShoppingCartController : Controller
    {
        public IActionResult Index()
        {
            return RedirectToAction("Index", "Cart");
        }
    }
}
