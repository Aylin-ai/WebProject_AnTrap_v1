using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Security.Claims;
using WebVersion.Models;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using WebVersion.Pages;

namespace WebVersion.ViewComponents
{
    public class UserButton : ViewComponent
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public UserButton(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        [HttpGet]
        public async Task<IViewComponentResult> InvokeAsync()
        {
            try
            {
                var authenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync("MyCookieAuthenticationScheme");

                if (authenticateResult.Succeeded && authenticateResult.Principal != null)
                {
                    var principal = authenticateResult.Principal;

                    // Получение утверждения имени пользователя
                    var imageClaim = principal.FindFirst(ClaimTypes.Surname);
                    var userImage = imageClaim?.Value;

                    object? model = userImage;
                    return View("_UserButton", model);
                }
                return View("_UserButton");
            }
            catch (Exception ex)
            {
                return View("_UserButton");
            }
        }
    }
}
