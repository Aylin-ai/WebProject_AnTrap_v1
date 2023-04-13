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
        public int AnimeId { get; set; }
        public ShikimoriSharp.Enums.Order Order { get; set; } = ShikimoriSharp.Enums.Order.ranked;
        private IHttpClientFactory _httpClientFactory;
        private ILogger logger;
        public List<Anime> AnimeList { get; set; } = new List<Anime>();

        public List<SelectListItem> AnimeTypes { get; set; } = new List<SelectListItem>();
        public string Type { get; set; }

        public List<SelectListItem> AnimeStatuses { get; set; } = new List<SelectListItem>();
        public string Status { get; set; }

        public List<SelectListItem> AnimeGenres { get; set; } = new List<SelectListItem>();
        public int Genre { get; set; }

        public List<SelectListItem> PagesId { get; set; } = new List<SelectListItem>();

        public MainAnimePageModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            for (int i = 1; i <= 406; i++)
            {
                PagesId.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
            }
            AnimeTypes.AddRange(new List<SelectListItem>() 
                                   {
                                   new SelectListItem { Value = "", Text = "Всё" },
                                   new SelectListItem { Value = "tv", Text = "TV"},
                                   new SelectListItem { Value = "movie", Text = "Фильм" },
                                   new SelectListItem { Value = "ova", Text = "OVA" },
                                   new SelectListItem { Value = "ona", Text = "ONA" },
                                   new SelectListItem { Value = "tv_13", Text = "TV 13 серий" },
                                   new SelectListItem { Value = "tv_24", Text = "TV 24 серии" },
                                   new SelectListItem { Value = "tv_48", Text = "TV 48 серий" }
                                   });

            AnimeStatuses.AddRange(new List<SelectListItem>()
            {
                new SelectListItem { Value = "", Text = "Всё" },
                new SelectListItem { Value = "anons", Text = "Анонс" },
                new SelectListItem { Value = "ongoing", Text = "Онгоинг" },
                new SelectListItem { Value = "released", Text = "Вышел" }
            });


        }

        public async Task OnGetAsync()
        {
            await GetAnimes(1, Order);
        }

        public async Task GetAnimes(int page, ShikimoriSharp.Enums.Order order, string type = "", string status = "", int genre = 0)
        {
            var client = new ShikimoriClient(logger, new ClientSettings("ShikiOAuthTest", APIToken.clientID, APIToken.clientSecret));

            Anime[] search;
            var genres = await client.Genres.GetGenres();
            AnimeGenres.Add(new SelectListItem { Value = "0", Text = "Всё" });
            for (int i = 0; i < genres.Length; i++)
            {
                AnimeGenres.Add(new SelectListItem { Value = genres[i].Id.ToString(), Text = genres[i].Russian.ToString() });
            }
            if (genre == 0)
            {
                search = await client.Animes.GetAnime(new ShikimoriSharp.Settings.AnimeRequestSettings
                {
                    limit = 50,
                    order = order,
                    page = page,
                    kind = type,
                    status = status,
                });
            }
            else
            {
                int[] genresId = { genre };
                search = await client.Animes.GetAnime(new ShikimoriSharp.Settings.AnimeRequestSettings
                {
                    limit = 50,
                    order = order,
                    page = page,
                    kind = type,
                    status = status,
                    genre = genresId,
                });
            }
            AnimeList = search.ToList();
        }

        public async Task OnPostAnimesById(int id, int order, string type, string status, int genre)
        {
            Id = id;
            switch (order)
            {
                case 1: Order = ShikimoriSharp.Enums.Order.ranked; break;
                case 2: Order = ShikimoriSharp.Enums.Order.kind; break;
                case 3: Order = ShikimoriSharp.Enums.Order.popularity; break;
                case 4: Order = ShikimoriSharp.Enums.Order.name; break;
                case 5: Order = ShikimoriSharp.Enums.Order.aired_on; break;
                case 6: Order = ShikimoriSharp.Enums.Order.status; break;
                case 7: Order = ShikimoriSharp.Enums.Order.random; break;
                default: Order = ShikimoriSharp.Enums.Order.ranked; break;
            }
            Type = type;
            Status = status;
            Genre = genre;
            await GetAnimes(Id, Order, Type, Status, Genre);
        }

        public IActionResult OnPostAnimeIdPage(int id)
        {
            return RedirectToPage("/AnimeId", new { animeId = id });
        }
    }
}
