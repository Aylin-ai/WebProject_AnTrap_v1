using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using WebVersion.AdditionalClasses;

namespace WebVersion.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        public string ErrorMessage { get; set; }

        public IndexModel(ILogger<IndexModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }

        public IActionResult OnPostRegistration(string Login, string Password1, string Password2)
        {
            if (Login == null || Password1 == null || Password2 == null)
            {
                ErrorMessage = "Вы ввели не все данные";
                return Page();
            }
            else
            {
                if (Password1 != Password2)
                {
                    ErrorMessage = "Пароли не совпадают";
                    return Page();
                }
                MySqlConnection conn = DBUtils.GetDBConnection();
                conn.Open();
                try
                {
                    string sql = "insert into userinformation " +
                        "(UserInformation_Login, UserInformation_Password) " +
                        "values (@login, @password);";

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.CommandText = sql;
                    cmd.Connection = conn;

                    cmd.Parameters.AddWithValue("@login", Login);
                    cmd.Parameters.AddWithValue("@password", Password1);

                    cmd.ExecuteNonQuery();
                    return RedirectToPage("/UserProfile", new { login = Login });
                }
                catch (Exception ex)
                {
                    return RedirectToPage("Error");
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        public IActionResult OnPostAuthorization(string Login, string Password1)
        {
            if (Login == null || Password1 == null)
            {
                ErrorMessage = "Вы ввели не все данные";
                return Page();
            }
            else
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

                    cmd.Parameters.AddWithValue("@login", Login);

                    var reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (Login == reader.GetString(0) && Password1 == reader.GetString(1))
                                return RedirectToPage("/UserProfile", new { login = Login });
                        }
                    }
                    return Page();
                }
                catch (Exception ex)
                {
                    return RedirectToPage("Error");
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }
    }
}