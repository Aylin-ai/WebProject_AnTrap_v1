using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using WebVersion.AdditionalClasses;

namespace WebVersion.Pages
{
    public class UserListModel : PageModel
    {
        public List<User> Users { get; set; } = new List<User>();

        public async Task OnGetAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                await GetUsers();
            }
        }

        public async Task<IActionResult> OnPostDeleteUser(int id)
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "delete from userinformation where Id = @id";
                cmd.CommandText = sql;

                cmd.Parameters.AddWithValue("@id", id);

                await cmd.ExecuteNonQueryAsync();
                return Page();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        public async Task GetUsers()
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select * from userinformation where UserRole_Id = 1";
                cmd.CommandText = sql;

                var users = new List<User>();

                var reader = await cmd.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        users.Add(new User()
                        {
                            Id = reader.GetInt32(0),
                            Login = reader.GetString(1),
                            Password = reader.GetString(2),
                            Email = reader.GetString(3),
                            ImageSource = reader.GetString(4),
                        });
                    }
                    Users = users;
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
