using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebVersion.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public IActionResult OnPostRegistration(string Login, string Password1, string Password2)
        {
            string registrationData = $"Login: {Login}, Password1: {Password1}, Password2: {Password2}";
            return Content(registrationData);
        }

        public IActionResult OnPostAuthorization(string Login, string Password1)
        {
            string registrationData = $"Login: {Login}, Password: {Password1}";
            return Content(registrationData);
        }
    }
}