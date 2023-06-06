using Firebase.Database;
using FirebaseAdmin;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using ShikimoriSharp.Classes;
using System.Net.Http;
using System.Security.Claims;
using WebVersion.Models;

namespace WebVersion.Pages
{
    public class MangaListModel : PageModel
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private FirebaseApp app;

        public List<MangaID> MangaList { get; set; } = new List<MangaID>();

        public List<int> CountOfAnimeInList { get; set; } = new List<int>();

        public List<string> SelectedList { get; set; } = new List<string>();

        public MangaListModel(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            app = FirebaseAppProvider.GetFirebaseApp();
        }

        public async void OnGetAsync()
        {
        }

        public async Task OnPostMangaListSelect(string selectedList)
        {
            await GetManga(selectedList);
        }

        public IActionResult OnPostMangaIdPage(int id)
        {
            return RedirectToPage("/MangaId", new { mangaId = id });
        }

        public IActionResult OnPostToSettings()
        {
            return RedirectToPage("/UserProfile");
        }

        public async Task<IEnumerable<FirebaseObject<PieceInList>>> GetManga(string selectedList = "Читаю", string Email = null)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://shikimori.me");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("User-Agent", "ShikiOAuthTest");

            if (Email != null)
            {
                var firebase = new FirebaseClient("https://antrap-firebase-default-rtdb.firebaseio.com/");
                try
                {
                    var result = await firebase.Child("manga").OnceAsync<PieceInList>();
                    var filteredResult = result.Where(item => item.Object.userEmail == Email && item.Object.userList == selectedList);
                    var mangaList = new List<MangaID>();
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
                        mangaList.Add(await httpClient.GetFromJsonAsync<MangaID>($"/api/mangas/{data.pieceId}"));
                        SelectedList.Add($"{userList} {data.pieceId}");
                    }
                    return filteredResult;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return null;
                }
            }
            else
            {
                var authenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync("MyCookieAuthenticationScheme");

                if (authenticateResult.Succeeded && authenticateResult.Principal != null)
                {

                    MangaList.Clear();

                    var principal1 = authenticateResult.Principal;

                    // Получение утверждения имени пользователя
                    var emailClaim = principal1.FindFirst(ClaimTypes.Email);
                    var email = emailClaim?.Value;
                    var firebase = new FirebaseClient("https://antrap-firebase-default-rtdb.firebaseio.com/");
                    try
                    {
                        var result = await firebase.Child("manga").OnceAsync<PieceInList>();
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
                            MangaList.Add(await httpClient.GetFromJsonAsync<MangaID>($"/api/mangas/{data.pieceId}"));
                            SelectedList.Add($"{userList} {data.pieceId}");
                        }
                        return filteredResult;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        return null;
                    }
                }
                return null;
            }
        } 
    }
}
