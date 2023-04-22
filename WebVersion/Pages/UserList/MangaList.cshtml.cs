using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using ShikimoriSharp.Classes;
using WebVersion.AdditionalClasses;

namespace WebVersion.Pages.UserList
{
    public class MangaListModel : PageModel
    {
        private IHttpClientFactory _httpClientFactory;
        public List<MangaID> MangaList { get; set; } = new List<MangaID>();

        public List<int> CountOfAnimeInList { get; set; } = new List<int>();

        public List<string> SelectedList { get; set; } = new List<string>();

        public MangaListModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async void OnGetAsync()
        {
        }

        public async Task OnPostMangaListSelect(string selectedList)
        {
            await GetManga(selectedList);
        }

        public IActionResult OnPostMangaIdPage(int id)
        {
            return RedirectToPage("/MangaId", new { mangaId = id });
        }

        public IActionResult OnPostToSettings()
        {
            return RedirectToPage("/UserProfile");
        }

        public async Task GetManga(string selectedList = "Читаю")
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://shikimori.me");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("User-Agent", "ShikiOAuthTest");

            MangaList.Clear();
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select Piece_PieceId from piece " +
                                    "where Piece_UserInformation_Login = @login and " +
                                    "Piece_ListInformation_Id = @listId and " +
                                    "Piece_Kind = 'манга'";

                int listId = 1;
                switch (selectedList)
                {
                    case "Читаю":
                        listId = 1;
                        break;
                    case "В планах":
                        listId = 2;
                        break;
                    case "Брошено":
                        listId = 3;
                        break;
                    case "Прочитано":
                        listId = 4;
                        break;
                    case "Любимые":
                        listId = 5;
                        break;
                }
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@login", User.Identity.Name);
                cmd.Parameters.AddWithValue("@listId", listId);

                var reader = await cmd.ExecuteReaderAsync();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        MangaList.Add(await httpClient.GetFromJsonAsync<MangaID>($"/api/mangas/{reader.GetInt32(0)}"));
                        SelectedList.Add($"{listId} {reader.GetInt32(0)}");
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
