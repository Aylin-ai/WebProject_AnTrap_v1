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

        public IActionResult OnPostAnimeIdPage(int id)
        {
            return RedirectToPage("/AnimeId", new { animeId = id });
        }

        public IActionResult OnPostMangaIdPage(int id)
        {
            return RedirectToPage("/MangaId", new { mangaId = id });
        }
    }
}
