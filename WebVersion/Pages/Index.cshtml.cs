using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using ShikimoriSharp.Classes;
using System.Security.Claims;
using WebVersion.AdditionalClasses;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Builder.Extensions;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using FirebaseAdmin.Auth;
using Firebase.Auth.Providers;
using static Google.Apis.Auth.OAuth2.Web.AuthorizationCodeWebApp;

namespace WebVersion.Pages
{
    public class IndexModel : PageModel
    {
        private string apiKey = "AIzaSyBbHCakvrudhkbVFq0YqQaZjzy8KpR01vM";
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public string ErrorMessage { get; set; }
        public string UserImageSrc { get; set; }
        private int _role;
        private string userName = "";
        private string userEmail = "";
        private string _userImage = "";
        private string idToken = "";
        private string localId = "";
        private string refreshIdToken = "";
        private FirebaseApp app;

        public IndexModel(ILogger<IndexModel> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            app = FirebaseAppProvider.GetFirebaseApp();
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostRegistration(string Login, string Password1, string Password2, string Email)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            if (Login == null || Password1 == null || Password2 == null)
            {
                ErrorMessage = "Вы ввели не все данные";
                return Page();
            }
            else
            {
                if (Password1 != Password2)
                {
                    ErrorMessage = "Пароли не совпадают";
                    return Page();
                }

                try
                {
                    var auth = FirebaseAuth.GetAuth(app);
                    var result = await auth.CreateUserAsync(new UserRecordArgs
                    {
                        Email = Email,
                        Password = Password1
                    });

                    var userId = result.Uid;

                    var user = new UserRecordArgs
                    {
                        Uid = userId,
                        DisplayName = Login, // Имя пользователя
                        PhotoUrl = "https://firebasestorage.googleapis.com/v0/b/antrap-firebase.appspot.com/o/OldPif.jpg?alt=media&token=6b117022-e75e-4b3c-b859-937f89516f8b" // Ссылка на фото пользователя
                    };

                    await auth.UpdateUserAsync(user);

                    localId = user.Uid;
                    userName = user.DisplayName;
                    _userImage = user.PhotoUrl;

                    // Обработка успешной регистрации и полученных данных пользователя
                    var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, $"{userName}"),
                                    new Claim(ClaimTypes.Surname, $"{_userImage}"),
                                    new Claim(ClaimTypes.Email, $"{Email}"),
                                    new Claim(ClaimTypes.Role, _role == 1 ? "Пользователь" : "Разработчик")
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

                    return RedirectToPage("/UserProfile", new { login = Email });
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    return Page();
                }
            }
        }

        public async Task<IActionResult> OnPostAuthorization(string Email, string Password1)
        {
            if (!ModelState.IsValid)
                return Page();
            if (Email == null || Password1 == null)
            {
                ErrorMessage = "Вы ввели не все данные";
                return Page();
            }
            else
            {
                try
                {
                    var client = new HttpClient();
                    using (var request = new HttpRequestMessage(new HttpMethod("POST"), $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={apiKey}"))
                    {
                        string json = $"{{\"email\":\"{Email}\",\"password\":\"{Password1}\",\"returnSecureToken\":true}}";
                        request.Content = new StringContent(json);
                        request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

                        var response = await client.SendAsync(request);
                        if (response.IsSuccessStatusCode)
                        {
                            var resultJson = await response.Content.ReadAsStringAsync();
                            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<SignInResponse>(resultJson);

                            try
                            {
                                var auth = FirebaseAuth.GetAuth(app);
                                var user = await auth.GetUserByEmailAsync(Email);

                                idToken = result.idToken;
                                localId = user.Uid;
                                userEmail = user.Email;
                                userName = user.DisplayName;
                                refreshIdToken = result.refreshToken;
                                _userImage = user.PhotoUrl;
                                _role = 1;
                            }
                            catch (FirebaseAuthException ex)
                            {
                                Console.WriteLine(ex.Message);
                            }
                            client.Dispose();
                            // Обработка успешного входа и полученных данных пользователя
                            var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, $"{userName}"),
                                    new Claim(ClaimTypes.Surname, $"{_userImage}"),
                                    new Claim(ClaimTypes.Email, $"{userEmail}"),
                                    new Claim(ClaimTypes.Role, _role == 1 ? "Пользователь" : "Разработчик")
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
                            return RedirectToPage("/UserProfile", new { login = Email });
                        }
                        else
                        {
                            return Page();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    return Page();
                }
            }
        }
    }
}