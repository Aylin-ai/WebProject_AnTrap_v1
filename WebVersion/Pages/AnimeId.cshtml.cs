using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ShikimoriSharp.Bases;
using ShikimoriSharp;
using WebVersion.Models;
using ShikimoriSharp.Classes;
using ShikimoriSharp.AdditionalRequests;
using MySql.Data.MySqlClient;
using WebVersion.AdditionalClasses;

namespace WebVersion.Pages
{
    public class AnimeIdModel : PageModel
    {
        private IHttpClientFactory _httpClientFactory;

        [BindProperty(Name = "animeId", SupportsGet = true)]
        public int AnimeId { get; set; }
        public AnimeID? Anime { get; set; }
        public Related?[] Related { get; set; }
        public Anime[] Similar { get; set; }

        public List<string> SelectedList { get; set; } = new List<string>();

        public List<string> SelectedLists { get; set; } = new List<string>();
        public List<string> SimilarAnimeList { get; set; } = new List<string>();

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
            Related = await httpClient.GetFromJsonAsync<Related[]>($"/api/animes/{AnimeId}/related");
            Similar = await httpClient.GetFromJsonAsync<Anime[]>($"/api/animes/{AnimeId}/similar");
            await GetAnimeFromUserList();

            httpClient.Dispose();
            if (Anime == null)
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

        public async Task GetAnimeFromUserList()
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select * from anime " +
                    "where Anime_UserInformation_Login = @login " +
                    "and Anime_AnimeId = @animeId";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@login", User.Identity.Name);
                cmd.Parameters.AddWithValue("@animeId", Anime.Id);

                var reader = await cmd.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        SelectedList.Add($"{reader.GetInt32(3)} {Anime.Id}");
                    }
                }
                reader.Close();

                string sqlAnime = "select * from anime " +
                    "where Anime_UserInformation_Login = @login;";
                cmd.CommandText = sqlAnime;

                reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    for (int i = 0; i < Related.Count(); i++)
                    {
                        while (reader.Read())
                        {
                            if (Related[i].Anime != null)
                            {
                                if (Related[i].Anime.Id == reader.GetInt32(1))
                                    SelectedLists.Add($"{reader.GetInt32(3)} {reader.GetInt32(1)}");
                            }
                            else if (Related[i].Manga != null)
                            {
                                SelectedLists.Add($"{mangaReader.GetInt32(3)} {mangaReader.GetInt32(1)}");
                            }
                        }
                    }

                }
                reader.Close();

                cmd.CommandText = sqlAnime;
                reader = await cmd.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        if (Similar.Any(x => x.Id == reader.GetInt32(1)))
                        {
                            SimilarAnimeList.Add($"{reader.GetInt32(3)} " +
                                $"{(int)Similar.Where(x => x.Id == reader.GetInt32(1)).First().Id}");
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }
    }
}
