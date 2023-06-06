using Firebase.Database;
using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebVersion.Models;

namespace WebVersion.Pages
{
    public class UserListModel : PageModel
    {
        private FirebaseApp app;
        public List<User> Users { get; set; }

        public UserListModel()
        {
            app = FirebaseAppProvider.GetFirebaseApp();
            Task.Run(GetUsers);
        }

        public async void OnGetAsync()
        {
        }

        public async Task<IActionResult> OnPostDeleteUser(string id)
        {
            FirebaseAuth auth = FirebaseAuth.GetAuth(app);
            var httpClient = new HttpClient();
            var databaseUrl = "https://antrap-firebase-default-rtdb.firebaseio.com/";
            var user = await auth.GetUserAsync(id);
            var nodePath = $"users/{user.Email.Replace('.', ',')}.json";
            try
            {
                var userId = id;
                await auth.DeleteUserAsync(userId);

                var request = new HttpRequestMessage(HttpMethod.Delete, $"{databaseUrl}{nodePath}");

                // Отправьте HTTP запрос и получите ответ
                var response = await httpClient.SendAsync(request);

                // Проверьте статусный код ответа
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Данные успешно удалены из Firebase Realtime Database.");
                }
                else
                {
                    Console.WriteLine($"Произошла ошибка при удалении данных: {response.StatusCode}");
                }

                return RedirectToPage("UserList");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task GetUsers()
        {
            List<User> users = new List<User>();
            var firebase = new FirebaseClient("https://antrap-firebase-default-rtdb.firebaseio.com/");
            try
            {
                var result = await firebase.Child("users").OnceAsync<User>();
                foreach (var user in result)
                {
                    users.Add(user.Object);
                }
                Users = users;

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
            }
        }
    }
}
