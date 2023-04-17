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
            if (User.Identity.IsAuthenticated)
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
            else
            {
                return RedirectToPage("Index");
            }
        }

        public IActionResult OnPostRanobeIdPage(int id)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToPage("/RanobeId", new { ranobeId = id });
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
