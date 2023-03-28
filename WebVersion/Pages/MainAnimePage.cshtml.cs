using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ShikimoriSharp.Bases;
using ShikimoriSharp;
using System.Net.Http;
using WebVersion.Models;
using ShikimoriSharp.Classes;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebVersion.Pages
{
    public class MainAnimePageModel : PageModel
    {
        public int Id { get; set; } = 1;
        private IHttpClientFactory _httpClientFactory;
        private ILogger logger;
        public List<Anime> AnimeList { get; set; } = new List<Anime>();

        public List<SelectListItem> PagesId { get; set; } = new List<SelectListItem>();

        public MainAnimePageModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            for (int i = 1; i <= 406; i++)
            {
                PagesId.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
            }
        }

        public async Task OnGetAsync()
        {
            await GetAnimes(1);
        }

        public async Task GetAnimes(int page)
        {
            var client = new ShikimoriClient(logger, new ClientSettings("ShikiOAuthTest", APIToken.clientID, APIToken.clientSecret));
            var search = await client.Animes.GetAnime(new ShikimoriSharp.Settings.AnimeRequestSettings
            {
                limit = 50,
                order = ShikimoriSharp.Enums.Order.ranked,
                page = page
            });
            AnimeList = search.ToList();
        }

        public async Task<IActionResult> OnGetAnimesById(int id)
        {
            Id = id;
            await GetAnimes(Id);
            if (AnimeList == null)
            {
                return Content("Ошибка");
            }
            return Page();
        }
    }
}
