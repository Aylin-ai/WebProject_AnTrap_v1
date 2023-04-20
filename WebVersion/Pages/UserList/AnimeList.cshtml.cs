using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using MySql.Data.MySqlClient;
using ShikimoriSharp.Classes;
using System.Net.Http;
using WebVersion.AdditionalClasses;

namespace WebVersion.Pages.UserList
{
    public class AnimeListModel : PageModel
    {
        private IHttpClientFactory _httpClientFactory;
        public List<AnimeID> AnimeList { get; set; } = new List<AnimeID>();

        public List<int> CountOfAnimeInList { get; set; } = new List<int>();

        public List<string> SelectedList { get; set; } = new List<string>();

        public AnimeListModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async void OnGetAsync()
        {
            await GetAnime();
        }

        public async Task OnPostAnimeListSelect(string selectedList)
        {
            await GetAnime(selectedList);
        }

        public IActionResult OnPostAnimeIdPage(int id)
        {
            return RedirectToPage("/AnimeId", new { animeId = id });
        }

        public IActionResult OnPostToSettings()
        {
            return RedirectToPage("/UserProfile");
        }

        public async Task GetAnime(string selectedList = "Смотрю")
        {
            HttpClient httpClient = _httpClientFactory.CreateClient();
            httpClient.BaseAddress = new Uri("https://shikimori.me");
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("User-Agent", "ShikiOAuthTest");

            AnimeList.Clear();
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select Anime_AnimeId from anime " +
                    "where Anime_UserInformation_Login = @login and " +
                    "Anime_ListInformation_Id = @listId";

                int listId = 1;
                switch (selectedList)
                {
                    case "Смотрю":
                        listId = 1;
                        break;
                    case "В планах":
                        listId = 2;
                        break;
                    case "Брошено":
                        listId = 3;
                        break;
                    case "Просмотрено":
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
                        AnimeList.Add(await httpClient.GetFromJsonAsync<AnimeID>($"/api/animes/{reader.GetInt32(0)}"));
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
