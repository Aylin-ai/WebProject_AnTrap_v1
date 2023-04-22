using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using WebVersion.AdditionalClasses;

namespace WebVersion.ViewComponents
{
    public class UserButton : ViewComponent
    {
        [HttpGet]
        public async Task<IViewComponentResult> InvokeAsync()
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

                cmd.Parameters.AddWithValue("@login", User.Identity.Name);

                var reader = await cmd.ExecuteReaderAsync();

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        object? model = reader.GetString(4);
                        return View("_UserButton", model);
                    }
                }
                return View("_UserButton"); ;
            }
            catch (Exception ex)
            {
                return View("_UserButton");
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }
    }
}
