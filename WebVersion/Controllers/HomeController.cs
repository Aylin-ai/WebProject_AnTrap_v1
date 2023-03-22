using Microsoft.AspNetCore.Mvc;

namespace WebVersion.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UserProfile()
        {
            return View();
        }
    }
}
