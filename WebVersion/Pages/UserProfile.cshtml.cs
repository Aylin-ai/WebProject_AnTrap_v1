using Firebase.Auth;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using ShikimoriSharp.Classes;
using ShikimoriSharp.UpdatableInformation;
using System.Security.Claims;
using WebVersion.AdditionalClasses;

namespace WebVersion.Pages
{
    public class UserProfileModel : PageModel
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string ErrorMessage { get; set; } = "";

        [BindProperty(Name = "login", SupportsGet = true)]
        public string OldEmail { get; set; }
        public string OldImageSource { get; set; } = "";

        private FirebaseApp app;
        

        public UserProfileModel(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            app = FirebaseAppProvider.GetFirebaseApp();
        }

        public async Task OnGetAsync()
        {
            if (!User.Identity.IsAuthenticated)
            {
                RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostUserEdit(string UserLogin,
            string NewPassword1, string NewPassword2)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (NewPassword1 != NewPassword2)
                {
                    ErrorMessage = "Новые пароли не совпадают";
                    return RedirectToPage("UserProfile");
                }
                var auth = FirebaseAuth.GetAuth(app);
                try
                {
                    var authenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync("MyCookieAuthenticationScheme");

                    if (authenticateResult.Succeeded && authenticateResult.Principal != null)
                    {
                        var principal1 = authenticateResult.Principal;

                        // Получение утверждения имени пользователя
                        var emailClaim = principal1.FindFirst(ClaimTypes.Email);
                        OldEmail = emailClaim?.Value;
                    }
                    var result = await auth.GetUserByEmailAsync(OldEmail);

                    var userId = result.Uid;
                    if (UserLogin != null)
                    {
                        var user = new UserRecordArgs
                        {
                            Uid = userId,
                            DisplayName = UserLogin, // Имя пользователя
                        };
                        await auth.UpdateUserAsync(user);
                    }
                    if (NewPassword1 != null)
                    {
                        var user = new UserRecordArgs
                        {
                            Uid = userId,
                            Password = NewPassword1,
                        };
                        await auth.UpdateUserAsync(user);
                    }

                    var localId = result.Uid;
                    var userName = result.DisplayName;
                    var _userImage = result.PhotoUrl;
                    var role = 1;

                    // Обработка успешной регистрации и полученных данных пользователя
                    var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, $"{userName}"),
                                    new Claim(ClaimTypes.Surname, $"{_userImage}"),
                                    new Claim(ClaimTypes.Email, $"{OldEmail}"),
                                    new Claim(ClaimTypes.Role, role == 1 ? "Пользователь" : "Разработчик")
                                };

                    var identity = new ClaimsIdentity(
                        claims, "MyCookieAuthenticationScheme");

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                    };

                    var principal = new ClaimsPrincipal(identity);

                    await _httpContextAccessor.HttpContext.SignInAsync(
                        "MyCookieAuthenticationScheme",
                        principal,
                        authProperties);

                    return RedirectToPage("UserProfile");
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    return RedirectToPage("UserProfile");
                }
            }
            else
            {
                return RedirectToPage("Index");
            }
        }


        public async Task<IActionResult> OnPostDeleteAccount()
        {
            if (User.Identity.IsAuthenticated)
            {
                var auth = FirebaseAuth.GetAuth(app);
                try
                {
                    var authenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync("MyCookieAuthenticationScheme");

                    if (authenticateResult.Succeeded && authenticateResult.Principal != null)
                    {
                        var principal1 = authenticateResult.Principal;

                        // Получение утверждения имени пользователя
                        var emailClaim = principal1.FindFirst(ClaimTypes.Email);
                        OldEmail = emailClaim?.Value;
                    }
                    var result = await auth.GetUserByEmailAsync(OldEmail);

                    var userId = result.Uid;
                    await auth.DeleteUserAsync(userId);
                    await HttpContext.SignOutAsync("MyCookieAuthenticationScheme");
                    return RedirectToPage("Index");
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    return Page();
                }
            }
            else
                return RedirectToPage("Index");
        }

        public IActionResult OnPostAboutAntrap()
        {
            return Content("AnTrap - это площадка, на которой вы можете составлять свои списки " +
                "аниме, манги и ранобэ, а также смотреть информацию о них");
        }

        public IActionResult OnPostCallCenter()
        {
            return Content("Если у вас возникли вопросы, пишите по следующему email адресу: " +
                "xportbfgh2821@gmail.com");
        }
    }
}
