using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ShikimoriSharp.Bases;
using ShikimoriSharp;
using WebVersion.Models;
using ShikimoriSharp.Classes;
using ShikimoriSharp.AdditionalRequests;

namespace WebVersion.Pages
{
    public class AnimeIdModel : PageModel
    {
        private IHttpClientFactory _httpClientFactory;

        [BindProperty(Name = "animeId", SupportsGet = true)]
        public int AnimeId { get; set; }
        public AnimeID? Anime { get; set; }
        public Related?[] RelatedAnime { get; set; }
        public Anime[] SimilarAnime { get; set; }

        public AnimeIdModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://shikimori.me");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("User-Agent", "ShikiOAuthTest");

            Anime = await httpClient.GetFromJsonAsync<AnimeID>($"/api/animes/{AnimeId}");
            Anime.Screens = await httpClient.GetFromJsonAsync<Screenshots[]>($"/api/animes/{AnimeId}/screenshots");
            RelatedAnime = await httpClient.GetFromJsonAsync<Related[]>($"/api/animes/{AnimeId}/related");
            SimilarAnime = await httpClient.GetFromJsonAsync<Anime[]>($"/api/animes/{AnimeId}/similar");
            if (Anime == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
