using Firebase.Database;
using FirebaseAdmin;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using ShikimoriSharp.Classes;
using System.Net.Http;
using System.Security.Claims;
using WebVersion.AdditionalClasses;

namespace WebVersion.Pages
{
    public class AnimeListModel : PageModel
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private FirebaseApp app;

        public List<AnimeID> AnimeList { get; set; } = new List<AnimeID>();

        public List<string> SelectedList { get; set; } = new List<string>();

        public AnimeListModel(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            app = FirebaseAppProvider.GetFirebaseApp();
        }

        public async void OnGetAsync()
        {
        }

        public async Task OnPostAnimeListSelect(string selectedList)
        {
            await GetAnime(selectedList);
        }

        public IActionResult OnPostAnimeIdPage(int id)
        {
            return RedirectToPage("/AnimeId", new { animeId = id });
        }

        public IActionResult OnPostToSettings()
        {
            return RedirectToPage("/UserProfile");
        }

        public async Task GetAnime(string selectedList = "Смотрю")
        {
            var authenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync("MyCookieAuthenticationScheme");

            if (authenticateResult.Succeeded && authenticateResult.Principal != null)
            {
                HttpClient httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri("https://shikimori.me");
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("User-Agent", "ShikiOAuthTest");

                AnimeList.Clear();

                var principal1 = authenticateResult.Principal;

                // Получение утверждения имени пользователя
                var emailClaim = principal1.FindFirst(ClaimTypes.Email);
                var email = emailClaim?.Value;
                var firebase = new FirebaseClient("https://antrap-firebase-default-rtdb.firebaseio.com/");
                try
                {
                    var result = await firebase.Child("anime").OnceAsync<PieceInList>();
                    var filteredResult = result.Where(item => item.Object.userEmail == email && item.Object.userList == selectedList);

                    foreach (var item in filteredResult)
                    {
                        var data = item.Object; // Данные из базы данных
                        int userList = 0;
                        switch (data.userList)
                        {
                            case "Читаю":
                                userList = 1;
                                break;
                            case "В планах":
                                userList = 2;
                                break;
                            case "Брошено":
                                userList = 3;
                                break;
                            case "Прочитано":
                                userList = 4;
                                break;
                            case "Любимое":
                                userList = 5;
                                break;
                        }
                        AnimeList.Add(await httpClient.GetFromJsonAsync<AnimeID>($"/api/animes/{data.pieceId}"));
                        SelectedList.Add($"{userList} {data.pieceId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
