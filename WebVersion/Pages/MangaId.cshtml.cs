using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShikimoriSharp.AdditionalRequests;
using ShikimoriSharp.Classes;

namespace WebVersion.Pages
{
    public class MangaIdModel : PageModel
    {
        private IHttpClientFactory _httpClientFactory;

        [BindProperty(Name = "mangaId", SupportsGet = true)]
        public int Id { get; set; }
        public MangaID? Manga { get; set; }
        public Related?[] Related { get; set; }
        public Manga[] Similar { get; set; }

        public MangaIdModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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
    }
}
