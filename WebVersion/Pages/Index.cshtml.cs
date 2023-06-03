using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;
using WebVersion.AdditionalClasses;
using System.Net.Http.Headers;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Firebase.Database;
using System.Text;

namespace WebVersion.Pages
{
    public class IndexModel : PageModel
    {
        private string apiKey = "AIzaSyBbHCakvrudhkbVFq0YqQaZjzy8KpR01vM";
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public string ErrorMessage { get; set; }
        public string UserImageSrc { get; set; }
        private string _role;
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

                    var firebase = new FirebaseClient("https://antrap-firebase-default-rtdb.firebaseio.com/");
                    var httpClient = new HttpClient();
                    var databaseUrl = "https://antrap-firebase-default-rtdb.firebaseio.com/";
                    var nodePath = $"users/{Email.Replace('.', ',')}.json";
                    User newUser = new User()
                    {
                        Id = userId,
                        Login = user.DisplayName,
                        Email = Email,
                        ImageSource = user.PhotoUrl,
                        Role = "Пользователь"
                    };
                    // Преобразуйте объект с данными в JSON строку
                    var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(newUser);

                    // Создайте HTTP запрос типа POST
                    var request = new HttpRequestMessage(HttpMethod.Put, $"{databaseUrl}{nodePath}")
                    {
                        Content = new StringContent(jsonData)
                    };

                    // Установите заголовок "Content-Type" для указания типа содержимого
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    // Отправьте HTTP запрос и получите ответ
                    var response = await httpClient.SendAsync(request);

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Данные успешно отправлены в Firebase Realtime Database.");
                    }
                    else
                    {
                        Console.WriteLine($"Произошла ошибка при отправке данных: {response.StatusCode}");
                    }

                    // Обработка успешной регистрации и полученных данных пользователя
                    var claims = new List<Claim>
                                {
                                    new Claim(ClaimTypes.Name, $"{userName}"),
                                    new Claim(ClaimTypes.Surname, $"{_userImage}"),
                                    new Claim(ClaimTypes.Email, $"{Email}"),
                                    new Claim(ClaimTypes.Role, "Пользователь")
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

                                localId = user.Uid;
                                userEmail = user.Email;
                                userName = user.DisplayName;
                                refreshIdToken = result.refreshToken;
                                _userImage = user.PhotoUrl;

                                var firebase = new FirebaseClient("https://antrap-firebase-default-rtdb.firebaseio.com/");
                                var userResult = await firebase.Child("users").OnceAsync<User>();
                                var filteredResult = userResult.Where(item => item.Object.Email == Email);

                                _role = filteredResult.First().Object.Role;
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
                                    new Claim(ClaimTypes.Role, _role)
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