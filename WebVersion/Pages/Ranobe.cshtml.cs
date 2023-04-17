using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using ShikimoriSharp.Classes;
using WebVersion.AdditionalClasses;

namespace WebVersion.Pages
{
    public class RanobeModel : PageModel
    {
        public int Page { get; set; } = 1;
        public int Id { get; set; }
        public ShikimoriSharp.Enums.Order Order { get; set; } = ShikimoriSharp.Enums.Order.ranked;
        private IHttpClientFactory _httpClientFactory;
        private ILogger logger;
        public List<Ranobe> List { get; set; } = new List<Ranobe>();

        public List<SelectListItem> Statuses { get; set; } = new List<SelectListItem>();
        public string Status { get; set; }

        public List<SelectListItem> Genres { get; set; } = new List<SelectListItem>();
        public int Genre { get; set; }

        public List<SelectListItem> Pages { get; set; } = new List<SelectListItem>();

        public RanobeModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
            for (int i = 1; i <= 406; i++)
            {
                Pages.Add(new SelectListItem { Value = i.ToString(), Text = i.ToString() });
            }

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
            if (User.Identity.IsAuthenticated)
                await GetRanobe(1, Order);
            else
                RedirectToPage("Index");
        }

        public async Task GetRanobe(int page, ShikimoriSharp.Enums.Order order, string status = "", int genre = 0)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://shikimori.me");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("User-Agent", "ShikiOAuthTest");


            Ranobe[] search;
            var genres = await httpClient.GetFromJsonAsync<Genre[]>("/api/genres");
            Genres.Add(new SelectListItem { Value = "0", Text = "Всё" });
            for (int i = 0; i < genres.Length; i++)
            {
                Genres.Add(new SelectListItem { Value = genres[i].Id.ToString(), Text = genres[i].Russian.ToString() });
            }
            if (genre == 0)
            {
                search = await httpClient.GetFromJsonAsync<Ranobe[]>($"/api/ranobe?limit=50&order={order}&page={page}&status={status}");
            }
            else
            {
                string apiRequest = $"/api/mangas?limit=50&order={order}&page={page}&status={status}&genre={genre}";
                search = await httpClient.GetFromJsonAsync<Ranobe[]>(apiRequest);
            }
            List = search.ToList();
            httpClient.Dispose();
        }

        public async Task OnPostById(int id, int order, string status, int genre)
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
            Status = status;
            Genre = genre;
            await GetRanobe(Id, Order, Status, Genre);
        }

        public IActionResult OnPostRanobeIdPage(int id)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToPage("/RanobeId", new { ranobeId = id });
            else
                return RedirectToPage("Index");
        }
    }
}
