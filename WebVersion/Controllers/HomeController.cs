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

        public string AboutAnTrap()
        {
            return "О Antrap";
        }

        public IActionResult Registration(string Login, string Password1, string Password2)
        {
            string registrationData = $"Login: {Login}, Password1: {Password1}, Password2: {Password2}";
            return Content(registrationData);
        }

        public IActionResult Authorization(string Login, string Password1)
        {
            string registrationData = $"Login: {Login}, Password: {Password1}";
            return Content(registrationData);
        }
    }
}
