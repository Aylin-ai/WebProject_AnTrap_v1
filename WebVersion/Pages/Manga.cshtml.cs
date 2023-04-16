using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShikimoriSharp.Classes;

namespace WebVersion.Pages
{
    public class MangaModel : PageModel
    {
        public int Page { get; set; } = 1;
        public int Id { get; set; }
        public ShikimoriSharp.Enums.Order Order { get; set; } = ShikimoriSharp.Enums.Order.ranked;
        private IHttpClientFactory _httpClientFactory;
        private ILogger logger;
        public List<Manga> List { get; set; } = new List<Manga>();

        public List<SelectListItem> Kinds { get; set; } = new List<SelectListItem>();
        public string Kind { get; set; }

        public List<SelectListItem> Statuses { get; set; } = new List<SelectListItem>();
        public string Status { get; set; }

        public List<SelectListItem> Genres { get; set; } = new List<SelectListItem>();
        public int Genre { get; set; }

        public List<SelectListItem> Pages { get; set; } = new List<SelectListItem>();

        public MangaModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            for (int i = 1; i <= 406; i++)
            {
                Pages.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
            }
            Kinds.AddRange(new List<SelectListItem>()
                                   {
                                   new SelectListItem { Value = "", Text = "Всё" },
                                   new SelectListItem { Value = "manga", Text = "Манга"},
                                   new SelectListItem { Value = "manhwa", Text = "Манхва" },
                                   new SelectListItem { Value = "manhua", Text = "Манхуа" },
                                   new SelectListItem { Value = "one_shot", Text = "One Shot" },
                                   new SelectListItem { Value = "doujin", Text = "Додзинси" }
                                   });

            Statuses.AddRange(new List<SelectListItem>()
            {
                new SelectListItem { Value = "", Text = "Всё" },
                new SelectListItem { Value = "anons", Text = "Анонс" },
                new SelectListItem { Value = "ongoing", Text = "Онгоинг" },
                new SelectListItem { Value = "released", Text = "Вышла" },
                new SelectListItem { Value = "paused", Text = "Заморожена" },
                new SelectListItem { Value = "discontinued", Text = "Остановлена" }
            });


        }

        public async Task OnGetAsync()
        {
            await GetMangas(1, Order);
        }

        public async Task GetMangas(int page, ShikimoriSharp.Enums.Order order, string type = "", string status = "", int genre = 0)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://shikimori.me");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("User-Agent", "ShikiOAuthTest");


            Manga[] search;
            var genres = await httpClient.GetFromJsonAsync<Genre[]>("/api/genres");
            Genres.Add(new SelectListItem { Value = "0", Text = "Всё" });
            for (int i = 0; i < genres.Length; i++)
            {
                Genres.Add(new SelectListItem { Value = genres[i].Id.ToString(), Text = genres[i].Russian.ToString() });
            }
            if (genre == 0)
            {
                search = await httpClient.GetFromJsonAsync<Manga[]>($"/api/mangas?limit=50&order={order}&page={page}&kind={type}&status={status}");
            }
            else
            {
                string apiRequest = $"/api/mangas?limit=50&order={order}&page={page}&kind={type}&status={status}&genre={genre}";
                search = await httpClient.GetFromJsonAsync<Manga[]>(apiRequest);
            }
            List = search.ToList();
            httpClient.Dispose();
        }

        public async Task OnPostById(int id, int order, string kind, string status, int genre)
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
            Kind = kind;
            Status = status;
            Genre = genre;
            await GetMangas(Id, Order, Kind, Status, Genre);
        }

        public IActionResult OnPostMangaIdPage(int id)
        {
            return RedirectToPage("/MangaId", new { mangaId = id });
        }
    }
}
