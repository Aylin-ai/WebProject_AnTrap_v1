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

        public string SelectedList { get; set; }

        public string[] SelectedLists { get; set; }
        public string[] SimilarAnimeList { get; set; }

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

                string sql = "select * from piece " +
                    "where Piece_UserInformation_Login = @login " +
                    "and Piece_PieceId = @animeId " +
                    "and Piece_Kind = 'аниме'";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@login", User.Identity.Name);
                cmd.Parameters.AddWithValue("@animeId", Anime.Id);

                var reader = await cmd.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        SelectedList = $"{reader.GetInt32(4)} {Anime.Id}";
                    }
                }
                reader.Close();

                string sqlAnime = "select * from piece " +
                    "where Piece_UserInformation_Login = @login and " +
                    "(Piece_Kind = 'манга' or Piece_Kind = 'аниме');";
                cmd.CommandText = sqlAnime;

                SelectedLists = new string[Related.Length];

                reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    for (int i = 0; i < Related.Length; i++)
                    {
                        reader.Close();
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            if (Related[i].Anime != null)
                            {
                                if (Related[i].Anime.Id == reader.GetInt32(2) && reader.GetString(1) == "аниме")
                                {
                                    SelectedLists[i] = ($"{reader.GetInt32(4)} {reader.GetInt32(2)}");
                                    break;
                                }
                                else
                                {
                                    SelectedLists[i] = ($"0 {Related[i].Anime.Id}");
                                }
                            }
                            else if (Related[i].Manga != null)
                            {
                                if (Related[i].Manga.Id == reader.GetInt32(2) && reader.GetString(1) == "манга")
                                {
                                    SelectedLists[i] = ($"{reader.GetInt32(4)} {reader.GetInt32(2)}");
                                    break;
                                }
                                else
                                {
                                    SelectedLists[i] = ($"0 {Related[i].Manga.Id}");
                                }
                            }
                        }
                    }
                }
                reader.Close();

                reader = await cmd.ExecuteReaderAsync();

                SimilarAnimeList = new string[Similar.Length > 8 ? 8 : Similar.Length];
                if (reader.HasRows)
                {
                    for (int i = 0; i < (Similar.Length > 8 ? 8 : Similar.Length); i++)
                    {
                        reader.Close();
                        reader = cmd.ExecuteReader();
                        while (reader.Read())
                        {
                            if (Similar[i].Id == reader.GetInt32(2) && reader.GetString(1) == "аниме")
                            {
                                SimilarAnimeList[i] = ($"{reader.GetInt32(4)} " +
                                    $"{Similar[i].Id}");
                                break;
                            }
                            else
                            {
                                SimilarAnimeList[i] = ($"0 {Similar[i].Id}");
                            }
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
