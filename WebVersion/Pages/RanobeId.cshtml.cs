using Firebase.Database;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using ShikimoriSharp.AdditionalRequests;
using ShikimoriSharp.Classes;
using System.Security.Claims;
using WebVersion.Models;

namespace WebVersion.Pages
{
    public class RanobeIdModel : PageModel
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;

        [BindProperty(Name = "ranobeId", SupportsGet = true)]
        public int Id { get; set; }
        public RanobeId? Ranobe { get; set; }
        public Related?[] Related { get; set; }
        public List<Ranobe> Similar { get; set; }

        public string SelectedList { get; set; }

        public string[] SelectedLists { get; set; }
        public string[] SimilarRanobeList { get; set; }

        public RanobeIdModel(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
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

                Ranobe = await httpClient.GetFromJsonAsync<RanobeId>($"/api/ranobe/{Id}");
                Related = await httpClient.GetFromJsonAsync<Related[]>($"/api/ranobe/{Id}/related");
                Ranobe[] similar = await httpClient.GetFromJsonAsync<Ranobe[]>($"/api/ranobe/{Id}/similar");
                Similar = similar.Where(x => x.Kind != "manga").ToList();
                await GetRanobeFromUserList();

                httpClient.Dispose();
                if (Ranobe == null)
                {
                    return NotFound();
                }
                return Page();
            }
            else
            {
                return RedirectToPage("Index");
            }
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

        public async Task GetRanobeFromUserList()
        {
            var authenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync("MyCookieAuthenticationScheme");

            if (authenticateResult.Succeeded && authenticateResult.Principal != null)
            {
                var principal1 = authenticateResult.Principal;

                // ��������� ����������� ����� ������������
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

                    if (filteredRanobeResult.Any())
                    {
                        int userList = 0;
                        foreach (var item in filteredRanobeResult)
                        {
                            if (item.Object.pieceId == Ranobe.Id)
                            {
                                switch (item.Object.userList)
                                {
                                    case "�����":
                                        userList = 1;
                                        break;
                                    case "� ������":
                                        userList = 2;
                                        break;
                                    case "�������":
                                        userList = 3;
                                        break;
                                    case "���������":
                                        userList = 4;
                                        break;
                                    case "�������":
                                        userList = 5;
                                        break;
                                }
                                SelectedList = $"{userList} {Ranobe.Id}";
                                break;
                            }
                            else
                            {
                                SelectedList = $"0 {Ranobe.Id}";
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
                                            case "������":
                                                userList = 1;
                                                break;
                                            case "� ������":
                                                userList = 2;
                                                break;
                                            case "�������":
                                                userList = 3;
                                                break;
                                            case "�����������":
                                                userList = 4;
                                                break;
                                            case "�������":
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
                                                case "�����":
                                                    userList = 1;
                                                    break;
                                                case "� ������":
                                                    userList = 2;
                                                    break;
                                                case "�������":
                                                    userList = 3;
                                                    break;
                                                case "���������":
                                                    userList = 4;
                                                    break;
                                                case "�������":
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
                                                case "�����":
                                                    userList = 1;
                                                    break;
                                                case "� ������":
                                                    userList = 2;
                                                    break;
                                                case "�������":
                                                    userList = 3;
                                                    break;
                                                case "���������":
                                                    userList = 4;
                                                    break;
                                                case "�������":
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

                    SimilarRanobeList = new string[Similar.Count > 8 ? 8 : Similar.Count];
                    for (int i = 0; i < (Similar.Count > 8 ? 8 : Similar.Count); i++)
                    {
                        if (filteredRanobeResult.Any())
                        {
                            int userList = 0;
                            foreach (var piece in filteredRanobeResult)
                            {
                                if (piece.Object.pieceId == Similar[i].Id)
                                {
                                    switch (piece.Object.userList)
                                    {
                                        case "������":
                                            userList = 1;
                                            break;
                                        case "� ������":
                                            userList = 2;
                                            break;
                                        case "�������":
                                            userList = 3;
                                            break;
                                        case "�����������":
                                            userList = 4;
                                            break;
                                        case "�������":
                                            userList = 5;
                                            break;
                                    }
                                    SimilarRanobeList[i] = ($"{userList} {piece.Object.pieceId}");
                                    break;
                                }
                                else
                                {
                                    SimilarRanobeList[i] = ($"0 {piece.Object.pieceId}");
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
