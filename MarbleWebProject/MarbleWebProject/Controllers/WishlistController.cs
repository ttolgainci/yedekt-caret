using Microsoft.AspNetCore.Mvc;

namespace MarbleWebProject.Controllers
{
    public class WishlistController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
