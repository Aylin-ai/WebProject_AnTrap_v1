using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
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

        public string SelectedList { get; set; }

        public string[] SelectedLists { get; set; }
        public string[] SimilarRanobeList { get; set; }

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
                Ranobe[] similar = await httpClient.GetFromJsonAsync<Ranobe[]>($"/api/ranobe/{Id}/similar");
                Similar = similar.Where(x => x.Kind != "manga").ToList();
                await GetRanobeFromUserList();

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

        public async Task GetRanobeFromUserList()
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select * from piece " +
                    "where Piece_UserInformation_Login = @login " +
                    "and Piece_PieceId = @ranobeId " +
                    "and Piece_Kind = 'ранобэ'";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@login", User.Identity.Name);
                cmd.Parameters.AddWithValue("@ranobeId", Ranobe.Id);

                var reader = await cmd.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        SelectedList = $"{reader.GetInt32(4)} {Ranobe.Id}";
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
                    "where Piece_UserInformation_Login = @login and Piece_Kind = 'ранобэ';";
                cmd.CommandText = sqlAnime;
                reader = await cmd.ExecuteReaderAsync();

                SimilarRanobeList = new string[Similar.Count > 8 ? 8 : Similar.Count];
                if (reader.HasRows)
                {
                    for (int i = 0; i < (Similar.Count > 8 ? 8 : Similar.Count); i++)
                    {
                        reader.Close();
                        reader = await cmd.ExecuteReaderAsync();
                        while (reader.Read())
                        {
                            if (Similar[i].Id == reader.GetInt32(2))
                            {
                                SimilarRanobeList[i] = ($"{reader.GetInt32(4)} " +
                                    $"{Similar[i].Id}");
                                break;
                            }
                            else
                            {
                                SimilarRanobeList[i] = ($"0 {Similar[i].Id}");
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
