using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ShikimoriSharp.Bases;
using ShikimoriSharp;
using System.Net.Http;
using WebVersion.Models;
using ShikimoriSharp.Classes;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using WebVersion.AdditionalClasses;

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

        public List<string> SelectedList { get; set; } = new List<string>();
        public Dictionary<int, int> AnimeInList { get; set; } = new Dictionary<int, int>();

        public string Search { get; set; }

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
            await GetAnimesFromUserList();
            await GetAnimes(1, Order);
        }

        public async Task GetAnimes(int page, ShikimoriSharp.Enums.Order order, string type = "", string status = "", int genre = 0)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://shikimori.me");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("User-Agent", "ShikiOAuthTest");


            Anime[] search;
            var genres = await httpClient.GetFromJsonAsync<Genre[]>("/api/genres");
            AnimeGenres.Add(new SelectListItem { Value = "0", Text = "Всё" });
            for (int i = 0; i < genres.Length; i++)
            {
                AnimeGenres.Add(new SelectListItem { Value = genres[i].Id.ToString(), Text = genres[i].Russian.ToString() });
            }
            if (genre == 0)
            {
                search = await httpClient.GetFromJsonAsync<Anime[]>($"/api/animes?limit=50&order={order}&page={page}&kind={type}&status={status}");
            }
            else
            {
                string apiRequest = $"/api/animes?limit=50&order={order}&page={page}&kind={type}&status={status}&genre={genre}";
                search = await httpClient.GetFromJsonAsync<Anime[]>(apiRequest);
            }
            AnimeList = search.ToList();
            httpClient.Dispose();

            foreach(var anime in AnimeList)
            {
                if (AnimeInList.Keys.Any(x => x == anime.Id))
                {
                    SelectedList.Add(
                        $"{AnimeInList.Where(x => x.Key == anime.Id).First().Value} " +
                        $"{AnimeInList.Where(x => x.Key == anime.Id).First().Key}"
                        );
                }
                else
                {
                    SelectedList.Add($"0 {anime.Id}");
                }
            }
        }

        public async Task GetAnimes(string searchAnime)
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://shikimori.me");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("User-Agent", "ShikiOAuthTest");

            var genres = await httpClient.GetFromJsonAsync<Genre[]>("/api/genres");
            AnimeGenres.Add(new SelectListItem { Value = "0", Text = "Всё" });
            for (int i = 0; i < genres.Length; i++)
            {
                AnimeGenres.Add(new SelectListItem { Value = genres[i].Id.ToString(), Text = genres[i].Russian.ToString() });
            }

            Anime[] search = new Anime[5];
            if (searchAnime != null)
            {
                search = await httpClient.GetFromJsonAsync<Anime[]>($"/api/animes?search={searchAnime}&limit=50");
            }
            AnimeList = search.ToList();
            httpClient.Dispose();

            foreach (var anime in AnimeList)
            {
                if (AnimeInList.Keys.Any(x => x == anime.Id))
                {
                    SelectedList.Add(
                        $"{AnimeInList.Where(x => x.Key == anime.Id).First().Value} " +
                        $"{AnimeInList.Where(x => x.Key == anime.Id).First().Key}"
                        );
                }
                else
                {
                    SelectedList.Add($"0 {anime.Id}");
                }
            }
        }

        public async Task OnPostAnimesById(int id, int order, string type, string status, int genre, string search)
        {
            if (search != null)
            {
                Search = search;
                await GetAnimes(Search);
            }
            else
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
        }

        public IActionResult OnPostAnimeIdPage(int id)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToPage("/AnimeId", new { animeId = id });
            else
                return RedirectToPage("/Index");
        }

        public async Task GetAnimesFromUserList()
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select * from piece " +
                                    "where UserInformation_Login = @login and " +
                                    "Kind = 'аниме'";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@login", User.Identity.Name);

                var reader = await cmd.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        AnimeInList[reader.GetInt32(2)] = reader.GetInt32(4);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }
    }
}
