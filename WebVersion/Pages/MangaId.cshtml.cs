using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using ShikimoriSharp.AdditionalRequests;
using ShikimoriSharp.Classes;
using WebVersion.AdditionalClasses;

namespace WebVersion.Pages
{
    public class MangaIdModel : PageModel
    {
        private IHttpClientFactory _httpClientFactory;

        [BindProperty(Name = "mangaId", SupportsGet = true)]
        public int Id { get; set; }
        public MangaID? Manga { get; set; }
        public Related?[] Related { get; set; }
        public Manga[] Similar { get; set; }

        public string SelectedList { get; set; }

        public string[] SelectedLists { get; set; }
        public string[] SimilarMangaList { get; set; }

        public MangaIdModel(IHttpClientFactory httpClientFactory)
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

                Manga = await httpClient.GetFromJsonAsync<MangaID>($"/api/mangas/{Id}");
                Related = await httpClient.GetFromJsonAsync<Related[]>($"/api/mangas/{Id}/related");
                Similar = await httpClient.GetFromJsonAsync<Manga[]>($"/api/mangas/{Id}/similar");
                await GetMangaFromUserList();

                httpClient.Dispose();
                if (Manga == null)
                {
                    return NotFound();
                }
                return Page();
            }
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

        public IActionResult OnPostRanobeIdPage(int id)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToPage("/RanobeId", new { ranobeId = id });
            else
                return RedirectToPage("Index");
        }


        public async Task GetMangaFromUserList()
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select * from piece " +
                    "where Piece_UserInformation_Login = @login " +
                    "and Piece_PieceId = @mangaId " +
                    "and Piece_Kind = 'манга'";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@login", User.Identity.Name);
                cmd.Parameters.AddWithValue("@mangaId", Manga.Id);

                var reader = await cmd.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        SelectedList = $"{reader.GetInt32(4)} {Manga.Id}";
                    }
                }
                reader.Close();

                string sqlAnime = "select * from piece " +
                    "where Piece_UserInformation_Login = @login and " +
                    "(Piece_Kind = 'манга' or Piece_Kind = 'аниме' or Piece_Kind = 'ранобэ');";
                cmd.CommandText = sqlAnime;

                SelectedLists = new string[Related.Length];

                reader = await cmd.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    for (int i = 0; i < Related.Length; i++)
                    {
                        reader.Close();
                        reader = await cmd.ExecuteReaderAsync();
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
                                if (Related[i].Manga.Id == reader.GetInt32(2) && (reader.GetString(1) == "манга"
                                    || reader.GetString(1) == "ранобэ"))
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

                sqlAnime = "select * from piece " +
                    "where Piece_UserInformation_Login = @login and Piece_Kind = 'манга';";
                cmd.CommandText = sqlAnime;
                reader = await cmd.ExecuteReaderAsync();

                SimilarMangaList = new string[Similar.Length > 8 ? 8 : Similar.Length];
                if (reader.HasRows)
                {
                    for (int i = 0; i < (Similar.Length > 8 ? 8 : Similar.Length); i++)
                    {
                        reader.Close();
                        reader = await cmd.ExecuteReaderAsync();
                        while (reader.Read())
                        {
                            if (Similar[i].Id == reader.GetInt32(2))
                            {
                                SimilarMangaList[i] = ($"{reader.GetInt32(4)} " +
                                    $"{Similar[i].Id}");
                                break;
                            }
                            else
                            {
                                SimilarMangaList[i] = ($"0 {Similar[i].Id}");
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
