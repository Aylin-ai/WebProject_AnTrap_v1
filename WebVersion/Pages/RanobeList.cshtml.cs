using Firebase.Database;
using FirebaseAdmin;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using ShikimoriSharp.Classes;
using System.Security.Claims;
using WebVersion.Models;

namespace WebVersion.Pages
{
    public class RanobeListModel : PageModel
    {
        private IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private FirebaseApp app;

        public List<RanobeId> RanobeList { get; set; } = new List<RanobeId>();

        public List<int> CountOfAnimeInList { get; set; } = new List<int>();

        public List<string> SelectedList { get; set; } = new List<string>();

        public RanobeListModel(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            app = FirebaseAppProvider.GetFirebaseApp();
        }

        public async void OnGetAsync()
        {
        }

        public async Task OnPostRanobeListSelect(string selectedList)
        {
            await GetRanobe(selectedList);
        }

        public IActionResult OnPostRanobeIdPage(int id)
        {
            return RedirectToPage("/RanobeId", new { ranobeId = id });
        }

        public IActionResult OnPostToSettings()
        {
            return RedirectToPage("/UserProfile");
        }

        public async Task GetRanobe(string selectedList = "�����")
        {
            var authenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync("MyCookieAuthenticationScheme");

            if (authenticateResult.Succeeded && authenticateResult.Principal != null)
            {
                HttpClient httpClient = _httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri("https://shikimori.me");
                httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("User-Agent", "ShikiOAuthTest");

                RanobeList.Clear();

                var principal1 = authenticateResult.Principal;

                // ��������� ����������� ����� ������������
                var emailClaim = principal1.FindFirst(ClaimTypes.Email);
                var email = emailClaim?.Value;
                var firebase = new FirebaseClient("https://antrap-firebase-default-rtdb.firebaseio.com/");
                try
                {
                    var result = await firebase.Child("ranobe").OnceAsync<PieceInList>();
                    var filteredResult = result.Where(item => item.Object.userEmail == email && item.Object.userList == selectedList);
                    foreach (var item in filteredResult)
                    {
                        var data = item.Object; // ������ �� ���� ������
                        int userList = 0;
                        switch (data.userList)
                        {
                            case "�����":
                                userList = 1;
                                break;
                            case "� ������":
                                userList = 2;
                                break;
                            case "�������":
                                userList = 3;
                                break;
                            case "���������":
                                userList = 4;
                                break;
                            case "�������":
                                userList = 5;
                                break;
                        }
                        RanobeList.Add(await httpClient.GetFromJsonAsync<RanobeId>($"/api/mangas/{data.pieceId}"));
                        SelectedList.Add($"{userList} {data.pieceId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
