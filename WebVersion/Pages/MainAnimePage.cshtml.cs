using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ShikimoriSharp.Bases;
using ShikimoriSharp;
using System.Net.Http;
using WebVersion.Models;
using ShikimoriSharp.Classes;

namespace WebVersion.Pages
{
    public class MainAnimePageModel : PageModel
    {
        private IHttpClientFactory _httpClientFactory;
        private ILogger logger;
        public List<Anime> AnimeList { get; set; } = new List<Anime>();

        public MainAnimePageModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task OnGetAsync()
        {
            await GetAnimes(23);
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

    }
}
