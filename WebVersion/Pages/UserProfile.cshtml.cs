using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using WebVersion.AdditionalClasses;

namespace WebVersion.Pages
{
    public class UserProfileModel : PageModel
    {
        public string ErrorMessage { get; set; } = "";
        [BindProperty(Name = "login", SupportsGet = true)]
        public string OldLogin { get; set; } = "";

        public string OldPassword { get; set; } = "";
        public string OldEmail { get; set; } = "";
        public string OldImageSource { get; set; } = "";

        public async Task OnGetAsync()
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();
            try
            {
                string sql = "select * from userinformation where " +
                    "UserInformation_Login = @login";

                MySqlCommand cmd = new MySqlCommand();
                cmd.CommandText = sql;
                cmd.Connection = conn;

                cmd.Parameters.AddWithValue("@login", OldLogin);

                var reader = cmd.ExecuteReader();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        OldPassword = reader.GetString(1);
                        OldEmail = reader.GetString(2);
                        OldImageSource = reader.GetString(3);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }
    }
}
