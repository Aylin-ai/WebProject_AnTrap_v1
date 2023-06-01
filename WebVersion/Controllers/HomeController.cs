using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Microsoft.AspNetCore.Authentication;
using WebVersion.AdditionalClasses;
using Microsoft.AspNetCore.SignalR;
using FirebaseAdmin;
using Firebase.Database;
using System.Security.Claims;
using System.Net.Http.Headers;
using System.IO.Pipelines;
using System.Text;

namespace WebVersion.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHubContext<NotificationHub> _hubContext;
        private IHttpClientFactory _httpClientFactory;
        private ILogger logger;
        private FirebaseApp app;


        public HomeController(IHttpClientFactory httpClientFactory, IHubContext<NotificationHub> hubContext, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _hubContext = hubContext;
            app = FirebaseAppProvider.GetFirebaseApp();
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> SendNotify(int userId, string message = "Уведомление")
        {
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", message);
            return Redirect(Request.Headers["Referer"].ToString());
        }


        public IActionResult Selected(string selectedValue)
        {
            if (User.Identity.IsAuthenticated)
            {
                switch (selectedValue)
                {
                    case "anime":
                        return RedirectToPage("/MainAnimePage");
                    case "manga":
                        return RedirectToPage("/Manga");
                    case "ranobe":
                        return RedirectToPage("/Ranobe");
                    default:
                        return RedirectToPage("/Error");
                }
            }
            else
                return RedirectToPage("/Index");
        }

        public async Task<IActionResult> SelectionFromUser(string selectedValue)
        {
            if (User.Identity.IsAuthenticated)
            {
                switch (selectedValue)
                {
                    case "userList":
                        return RedirectToPage("/UserList");
                    case "animeList":
                        return RedirectToPage("/AnimeList");
                    case "mangaList":
                        return RedirectToPage("/MangaList");
                    case "ranobeList":
                        return RedirectToPage("/RanobeList");
                    case "settings":
                        return RedirectToPage("/UserProfile");
                    case "logout":
                        await HttpContext.SignOutAsync("MyCookieAuthenticationScheme");
                        return RedirectToPage("/Index");
                    default:
                        return RedirectToPage("/Error");
                }
            }
            else
                return RedirectToPage("/Index");
        }
        public async Task<IActionResult> AddAnimeToList(string selectedList)
        {
            if (User.Identity.IsAuthenticated)
            {
                var authenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync("MyCookieAuthenticationScheme");

                if (authenticateResult.Succeeded && authenticateResult.Principal != null)
                {
                    var principal1 = authenticateResult.Principal;

                    // Получение утверждения имени пользователя
                    var emailClaim = principal1.FindFirst(ClaimTypes.Email);
                    var email = emailClaim?.Value;

                    string UserList = "";
                    switch (selectedList.Split(' ')[0])
                    {
                        case "1":
                            UserList = "Смотрю";
                            break;
                        case "2":
                            UserList = "В планах";
                            break;
                        case "3":
                            UserList = "Брошено";
                            break;
                        case "4":
                            UserList = "Просмотрено";
                            break;
                        case "5":
                            UserList = "Любимое";
                            break;
                    }

                    var firebase = new FirebaseClient("https://antrap-firebase-default-rtdb.firebaseio.com/");
                    var httpClient = new HttpClient();
                    var databaseUrl = "https://antrap-firebase-default-rtdb.firebaseio.com/";
                    var nodePath = $"anime/{selectedList.Split(' ')[1]} {email.Replace('.', ',')}.json";
                    try
                    {
                        var result = await firebase.Child("anime").OnceAsync<PieceInList>();
                        var filteredResult = result.Where(item => item.Object.userEmail == email);

                        if (selectedList.Split(" ")[0] == "0")
                        {
                            // Создайте HTTP запрос типа DELETE
                            var request = new HttpRequestMessage(HttpMethod.Delete, $"{databaseUrl}{nodePath}");

                            // Отправьте HTTP запрос и получите ответ
                            var response = await httpClient.SendAsync(request);

                            // Проверьте статусный код ответа
                            if (response.IsSuccessStatusCode)
                            {
                                Console.WriteLine("Данные успешно удалены из Firebase Realtime Database.");
                            }
                            else
                            {
                                Console.WriteLine($"Произошла ошибка при удалении данных: {response.StatusCode}");
                            }
                        }
                        else
                        {
                            if (!filteredResult.Any(x => x.Object.pieceId.ToString() == selectedList.Split(' ')[1]))
                            {
                                // Создайте объект с данными для отправки
                                PieceInList data = new PieceInList()
                                {
                                    pieceId = int.Parse(selectedList.Split(' ')[1]),
                                    userEmail = email,
                                    userList = UserList
                                };

                                // Преобразуйте объект с данными в JSON строку
                                var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);

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
                            }
                            else
                            {
                                PieceInList pieceInList = new PieceInList()
                                {
                                    pieceId = int.Parse(selectedList.Split(" ")[1]),
                                    userEmail = email,
                                    userList = UserList
                                };
                                var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(pieceInList);
                                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{databaseUrl}{nodePath}")
                                {
                                    Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
                                };
                                var response = await httpClient.SendAsync(request);

                                // Проверьте статусный код ответа
                                if (response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine("Данные успешно обновлены в Firebase Realtime Database.");
                                }
                                else
                                {
                                    Console.WriteLine($"Произошла ошибка при обновлении данных: {response.StatusCode}");
                                }
                            }
                        }
                        return Redirect(Request.Headers["Referer"].ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        return Redirect(Request.Headers["Referer"].ToString());
                    }
                }
                else
                    return RedirectToPage("/Index");
            }
            else
                return RedirectToPage("/Index");
        }
        public async Task<IActionResult> AddMangaToList(string selectedList)
        {
            if (User.Identity.IsAuthenticated)
            {
                var authenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync("MyCookieAuthenticationScheme");

                if (authenticateResult.Succeeded && authenticateResult.Principal != null)
                {
                    var principal1 = authenticateResult.Principal;

                    // Получение утверждения имени пользователя
                    var emailClaim = principal1.FindFirst(ClaimTypes.Email);
                    var email = emailClaim?.Value;

                    string UserList = "";
                    switch (selectedList.Split(' ')[0])
                    {
                        case "1":
                            UserList = "Читаю";
                            break;
                        case "2":
                            UserList = "В планах";
                            break;
                        case "3":
                            UserList = "Брошено";
                            break;
                        case "4":
                            UserList = "Прочитано";
                            break;
                        case "5":
                            UserList = "Любимое";
                            break;
                    }

                    var firebase = new FirebaseClient("https://antrap-firebase-default-rtdb.firebaseio.com/");
                    var httpClient = new HttpClient();
                    var databaseUrl = "https://antrap-firebase-default-rtdb.firebaseio.com/";
                    var nodePath = $"manga/{selectedList.Split(' ')[1]}%20{email.Replace('.', ',')}.json";
                    try
                    {
                        var result = await firebase.Child("manga").OnceAsync<PieceInList>();
                        var filteredResult = result.Where(item => item.Object.userEmail == email);

                        if (selectedList.Split(" ")[0] == "0")
                        {
                            // Создайте HTTP запрос типа DELETE
                            var request = new HttpRequestMessage(HttpMethod.Delete, $"{databaseUrl}{nodePath}");

                            // Отправьте HTTP запрос и получите ответ
                            var response = await httpClient.SendAsync(request);

                            // Проверьте статусный код ответа
                            if (response.IsSuccessStatusCode)
                            {
                                Console.WriteLine("Данные успешно удалены из Firebase Realtime Database.");
                            }
                            else
                            {
                                Console.WriteLine($"Произошла ошибка при удалении данных: {response.StatusCode}");
                            }
                        }
                        else
                        {
                            if (!filteredResult.Any(x => x.Object.pieceId.ToString() == selectedList.Split(' ')[1]))
                            {
                                // Создайте объект с данными для отправки
                                PieceInList data = new PieceInList()
                                {
                                    pieceId = int.Parse(selectedList.Split(' ')[1]),
                                    userEmail = email,
                                    userList = UserList
                                };

                                // Преобразуйте объект с данными в JSON строку
                                var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);

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
                            }
                            else
                            {
                                PieceInList pieceInList = new PieceInList()
                                {
                                    pieceId = int.Parse(selectedList.Split(" ")[1]),
                                    userEmail = email,
                                    userList = UserList
                                };
                                var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(pieceInList);
                                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{databaseUrl}{nodePath}")
                                {
                                    Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
                                };
                                var response = await httpClient.SendAsync(request);

                                // Проверьте статусный код ответа
                                if (response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine("Данные успешно обновлены в Firebase Realtime Database.");
                                }
                                else
                                {
                                    Console.WriteLine($"Произошла ошибка при обновлении данных: {response.StatusCode}");
                                }
                            }
                        }
                        httpClient.Dispose();
                        return Redirect(Request.Headers["Referer"].ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.InnerException.ToString()}");
                        return Redirect(Request.Headers["Referer"].ToString());
                    }
                }
                else
                    return RedirectToPage("/Index");
            }
            else
                return RedirectToPage("/Index");
        }
        public async Task<IActionResult> AddRanobeToList(string selectedList)
        {
            if (User.Identity.IsAuthenticated)
            {
                var authenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync("MyCookieAuthenticationScheme");

                if (authenticateResult.Succeeded && authenticateResult.Principal != null)
                {
                    var principal1 = authenticateResult.Principal;

                    // Получение утверждения имени пользователя
                    var emailClaim = principal1.FindFirst(ClaimTypes.Email);
                    var email = emailClaim?.Value;

                    string UserList = "";
                    switch (selectedList.Split(' ')[0])
                    {
                        case "1":
                            UserList = "Читаю";
                            break;
                        case "2":
                            UserList = "В планах";
                            break;
                        case "3":
                            UserList = "Брошено";
                            break;
                        case "4":
                            UserList = "Прочитано";
                            break;
                        case "5":
                            UserList = "Любимое";
                            break;
                    }

                    var firebase = new FirebaseClient("https://antrap-firebase-default-rtdb.firebaseio.com/");
                    var httpClient = new HttpClient();
                    var databaseUrl = "https://antrap-firebase-default-rtdb.firebaseio.com/";
                    var nodePath = $"ranobe/{selectedList.Split(' ')[1]} {email.Replace('.', ',')}.json";
                    try
                    {
                        var result = await firebase.Child("ranobe").OnceAsync<PieceInList>();
                        var filteredResult = result.Where(item => item.Object.userEmail == email);

                        if (selectedList.Split(" ")[0] == "0")
                        {
                            // Создайте HTTP запрос типа DELETE
                            var request = new HttpRequestMessage(HttpMethod.Delete, $"{databaseUrl}{nodePath}");

                            // Отправьте HTTP запрос и получите ответ
                            var response = await httpClient.SendAsync(request);

                            // Проверьте статусный код ответа
                            if (response.IsSuccessStatusCode)
                            {
                                Console.WriteLine("Данные успешно удалены из Firebase Realtime Database.");
                            }
                            else
                            {
                                Console.WriteLine($"Произошла ошибка при удалении данных: {response.StatusCode}");
                            }
                        }
                        else
                        {
                            if (!filteredResult.Any(x => x.Object.pieceId.ToString() == selectedList.Split(' ')[1]))
                            {
                                // Создайте объект с данными для отправки
                                PieceInList data = new PieceInList()
                                {
                                    pieceId = int.Parse(selectedList.Split(' ')[1]),
                                    userEmail = email,
                                    userList = UserList
                                };

                                // Преобразуйте объект с данными в JSON строку
                                var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);

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
                            }
                            else
                            {
                                PieceInList pieceInList = new PieceInList()
                                {
                                    pieceId = int.Parse(selectedList.Split(" ")[1]),
                                    userEmail = email,
                                    userList = UserList
                                };
                                var jsonData = Newtonsoft.Json.JsonConvert.SerializeObject(pieceInList);
                                var request = new HttpRequestMessage(new HttpMethod("PATCH"), $"{databaseUrl}{nodePath}")
                                {
                                    Content = new StringContent(jsonData, Encoding.UTF8, "application/json")
                                };
                                var response = await httpClient.SendAsync(request);

                                // Проверьте статусный код ответа
                                if (response.IsSuccessStatusCode)
                                {
                                    Console.WriteLine("Данные успешно обновлены в Firebase Realtime Database.");
                                }
                                else
                                {
                                    Console.WriteLine($"Произошла ошибка при обновлении данных: {response.StatusCode}");
                                }
                            }
                        }
                        return Redirect(Request.Headers["Referer"].ToString());
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error: {ex.Message}");
                        return Redirect(Request.Headers["Referer"].ToString());
                    }
                }
                else
                    return RedirectToPage("/Index");
            }
            else
                return RedirectToPage("/Index");
        }
    }
}
