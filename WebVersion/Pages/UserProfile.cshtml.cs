using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using ShikimoriSharp.Classes;
using ShikimoriSharp.UpdatableInformation;
using System.Security.Claims;
using WebVersion.AdditionalClasses;

namespace WebVersion.Pages
{
    public class UserProfileModel : PageModel
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public string ErrorMessage { get; set; } = "";
        private int _userId;
        private string _userName = "";
        private string _email = "";

        [BindProperty(Name = "login", SupportsGet = true)]
        public string OldLogin { get; set; } = "";

        public string OldEmail { get; set; } = "";
        public string OldImageSource { get; set; } = "";

        public UserProfileModel(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task OnGetAsync()
        {
            if (User.Identity.IsAuthenticated)
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
                            _userId = reader.GetInt32(0);
                            OldImageSource = reader.GetString(4);
                            OldEmail = reader.GetString(3);
                            _email = reader.GetString(3);
                            _userName = reader.GetString(1);
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
            else
            {
                RedirectToPage("Index");
            }
        }

        public async Task<IActionResult> OnPostUserEdit(string UserLogin, string UserEmail, 
            string NewPassword1, string NewPassword2)
        {
            UserLogin ??= _userName;
            UserEmail ??= _email;
            if (User.Identity.IsAuthenticated)
            {
                if (NewPassword1 != NewPassword2)
                {
                    ErrorMessage = "Новые пароли не совпадают";
                    return RedirectToPage("UserProfile");
                }

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
                            _userId = reader.GetInt32(0);
                            NewPassword1 ??= reader.GetString(2);
                        }
                    }
                    reader.Close();

                    sql = "select * from userinformation where " +
                        "UserInformation_Login != @login";
                    cmd.CommandText = sql;

                    reader = await cmd.ExecuteReaderAsync();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (UserLogin == reader.GetString(1))
                            {
                                ErrorMessage = $"Логин {UserLogin} уже занят";
                                return RedirectToPage("UserProfile");
                            }
                            if (UserEmail == reader.GetString(3))
                            {
                                ErrorMessage = $"Почта {UserEmail} уже занята";
                                return RedirectToPage("UserProfile");
                            }
                        }
                    }
                    reader.Close();
                    cmd.Parameters.Clear();

                    sql = "update userinformation " +
                        "set UserInformation_Login = @login, " +
                        "UserInformation_Password = @password, " +
                        "UserInformation_Email = @email " +
                        "where UserInformation_Id = @userId;";
                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@login", UserLogin);
                    cmd.Parameters.AddWithValue("password", NewPassword1);
                    cmd.Parameters.AddWithValue("@email", UserEmail);
                    cmd.Parameters.AddWithValue("@userId", _userId);

                    await cmd.ExecuteNonQueryAsync();

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, $"{UserLogin}"),
                        new Claim(ClaimTypes.Role, "User")
                    };

                    var identity = new ClaimsIdentity(
                        claims, "MyCookieAuthenticationScheme");

                    var authProperties = new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddMonths(1)
                    };

                    var principal = new ClaimsPrincipal(identity);

                    await _httpContextAccessor.HttpContext.SignInAsync(
                        "MyCookieAuthenticationScheme",
                        principal,
                        authProperties);

                    return RedirectToPage("UserProfile");
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    return RedirectToPage("UserProfile");
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            else
            {
                return RedirectToPage("Index");
            }
        }
    }
}
