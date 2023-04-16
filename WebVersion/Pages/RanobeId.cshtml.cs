using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ShikimoriSharp.AdditionalRequests;
using ShikimoriSharp.Classes;
using WebVersion.AdditionalClasses;

namespace WebVersion.Pages
{
    public class RanobeIdModel : PageModel
    {
        private IHttpClientFactory _httpClientFactory;

        [BindProperty(Name = "ranobeId", SupportsGet = true)]
        public int Id { get; set; }
        public RanobeId? Ranobe { get; set; }
        public Related?[] Related { get; set; }
        public List<Ranobe> Similar { get; set; }

        public RanobeIdModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://shikimori.me");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("User-Agent", "ShikiOAuthTest");

            Ranobe = await httpClient.GetFromJsonAsync<RanobeId>($"/api/ranobe/{Id}");
            Related = await httpClient.GetFromJsonAsync<Related[]>($"/api/ranobe/{Id}/related");
            IEnumerable<Ranobe> similar = await httpClient.GetFromJsonAsync<List<Ranobe>>($"/api/ranobe/{Id}/similar");
            Similar = similar.Where(x => x.Kind != "manga").ToList();
            httpClient.Dispose();
            if (Ranobe == null)
            {
                return NotFound();
            }
            return Page();
        }

        public IActionResult OnPostRanobeIdPage(int id)
        {
            return RedirectToPage("/RanobeId", new { ranobeId = id });
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
