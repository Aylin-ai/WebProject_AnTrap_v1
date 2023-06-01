using Firebase.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using ShikimoriSharp.AdditionalRequests;
using ShikimoriSharp.Classes;
using System.Security.Claims;
using WebVersion.AdditionalClasses;

namespace WebVersion.Pages
{
    public class MangaIdModel : PageModel
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;


        [BindProperty(Name = "mangaId", SupportsGet = true)]
        public int Id { get; set; }
        public MangaID? Manga { get; set; }
        public Related?[] Related { get; set; }
        public Manga[] Similar { get; set; }

        public string SelectedList { get; set; }

        public string[] SelectedLists { get; set; }
        public string[] SimilarMangaList { get; set; }

        public MangaIdModel(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                HttpClient httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri("https://shikimori.me");
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("User-Agent", "ShikiOAuthTest");

                Manga = await httpClient.GetFromJsonAsync<MangaID>($"/api/mangas/{Id}");
                Related = await httpClient.GetFromJsonAsync<Related[]>($"/api/mangas/{Id}/related");
                Similar = await httpClient.GetFromJsonAsync<Manga[]>($"/api/mangas/{Id}/similar");
                await GetMangaFromUserList();

                httpClient.Dispose();
                if (Manga == null)
                {
                    return NotFound();
                }
                return Page();
            }
            else
                return RedirectToPage("Index");
        }


        public IActionResult OnPostAnimeIdPage(int id)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToPage("/AnimeId", new { animeId = id });
            else
                return RedirectToPage("Index");
        }

        public IActionResult OnPostMangaIdPage(int id)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToPage("/MangaId", new { mangaId = id });
            else
                return RedirectToPage("Index");
        }

        public IActionResult OnPostRanobeIdPage(int id)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToPage("/RanobeId", new { ranobeId = id });
            else
                return RedirectToPage("Index");
        }

        public async Task GetMangaFromUserList()
        {
            var authenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync("MyCookieAuthenticationScheme");

            if (authenticateResult.Succeeded && authenticateResult.Principal != null)
            {
                var principal1 = authenticateResult.Principal;

                // Получение утверждения имени пользователя
                var emailClaim = principal1.FindFirst(ClaimTypes.Email);
                var email = emailClaim?.Value;
                var firebase = new FirebaseClient("https://antrap-firebase-default-rtdb.firebaseio.com/");
                try
                {
                    var animeResult = await firebase.Child("anime").OnceAsync<PieceInList>();
                    var filteredAnimeResult = animeResult.Where(item => item.Object.userEmail == email);

                    var mangaResult = await firebase.Child("manga").OnceAsync<PieceInList>();
                    var filteredMangaResult = mangaResult.Where(item => item.Object.userEmail == email);

                    var ranobeResult = await firebase.Child("ranobe").OnceAsync<PieceInList>();
                    var filteredRanobeResult = ranobeResult.Where(item => item.Object.userEmail == email);

                    if (filteredMangaResult.Any())
                    {
                        int userList = 0;
                        foreach (var item in filteredMangaResult)
                        {
                            if (item.Object.pieceId == Manga.Id)
                            {
                                switch (item.Object.userList)
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
                                SelectedList = $"{userList} {Manga.Id}";
                                break;
                            }
                            else
                            {
                                SelectedList = $"0 {Manga.Id}";
                            }
                        }
                    }

                    SelectedLists = new string[Related.Length];

                    for (int i = 0; i < Related.Length; i++)
                    {
                        if (Related[i].Anime != null)
                        {
                            if (filteredAnimeResult.Any())
                            {
                                int userList = 0;
                                foreach (var piece in filteredAnimeResult)
                                {
                                    if (piece.Object.pieceId == Related[i].Anime.Id)
                                    {
                                        switch (piece.Object.userList)
                                        {
                                            case "Смотрю":
                                                userList = 1;
                                                break;
                                            case "В планах":
                                                userList = 2;
                                                break;
                                            case "Брошено":
                                                userList = 3;
                                                break;
                                            case "Просмотрено":
                                                userList = 4;
                                                break;
                                            case "Любимое":
                                                userList = 5;
                                                break;
                                        }
                                        SelectedLists[i] = ($"{userList} {piece.Object.pieceId}");
                                        break;
                                    }
                                    else
                                    {
                                        SelectedLists[i] = ($"0 {piece.Object.pieceId}");
                                    }
                                }
                            }
                        }
                        else if (Related[i].Manga != null)
                        {
                            if (Related[i].Manga.Kind == "light_novel" || Related[i].Manga.Kind == "novel")
                            {
                                if (filteredRanobeResult.Any())
                                {
                                    int userList = 0;
                                    foreach (var piece in filteredRanobeResult)
                                    {
                                        if (piece.Object.pieceId == Related[i].Manga.Id)
                                        {
                                            switch (piece.Object.userList)
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
                                            SelectedLists[i] = ($"{userList} {piece.Object.pieceId}");
                                            break;
                                        }
                                        else
                                        {
                                            SelectedLists[i] = ($"0 {piece.Object.pieceId}");
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (filteredMangaResult.Any())
                                {
                                    int userList = 0;
                                    foreach (var piece in filteredMangaResult)
                                    {
                                        if (piece.Object.pieceId == Related[i].Manga.Id)
                                        {
                                            switch (piece.Object.userList)
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
                                            SelectedLists[i] = ($"{userList} {piece.Object.pieceId}");
                                            break;
                                        }
                                        else
                                        {
                                            SelectedLists[i] = ($"0 {piece.Object.pieceId}");
                                        }
                                    }
                                }
                            }
                        }
                    }

                    SimilarMangaList = new string[Similar.Length > 8 ? 8 : Similar.Length];
                    for (int i = 0; i < (Similar.Length > 8 ? 8 : Similar.Length); i++)
                    {
                        if (filteredMangaResult.Any())
                        {
                            int userList = 0;
                            foreach (var piece in filteredMangaResult)
                            {
                                if (piece.Object.pieceId == Similar[i].Id)
                                {
                                    switch (piece.Object.userList)
                                    {
                                        case "Смотрю":
                                            userList = 1;
                                            break;
                                        case "В планах":
                                            userList = 2;
                                            break;
                                        case "Брошено":
                                            userList = 3;
                                            break;
                                        case "Просмотрено":
                                            userList = 4;
                                            break;
                                        case "Любимое":
                                            userList = 5;
                                            break;
                                    }
                                    SimilarMangaList[i] = ($"{userList} {piece.Object.pieceId}");
                                    break;
                                }
                                else
                                {
                                    SimilarMangaList[i] = ($"0 {piece.Object.pieceId}");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message.ToString());
                }
            }
        }
    }
}
