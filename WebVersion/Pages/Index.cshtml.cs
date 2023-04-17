using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MySql.Data.MySqlClient;
using ShikimoriSharp.Classes;
using System.Security.Claims;
using WebVersion.AdditionalClasses;

namespace WebVersion.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public string ErrorMessage { get; set; }
        public string UserImageSrc { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostRegistration(string Login, string Password1, string Password2)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
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
                    string sql = "select * from userinformation where " +
                        "UserInformation_Login = @login";

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.CommandText = sql;
                    cmd.Connection = conn;

                    cmd.Parameters.AddWithValue("@login", Login);

                    var reader = cmd.ExecuteReader();
                    cmd.Parameters.Clear();

                    if (reader.HasRows)
                    {
                        ErrorMessage = "Пользователь с таким логином уже существует";
                        return Page();
                    }
                    else
                    {
                        await reader.CloseAsync();
                        sql = "insert into userinformation " +
                        "(UserInformation_Login, UserInformation_Password) " +
                        "values (@login, @password);";

                        cmd = new MySqlCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = conn;

                        cmd.Parameters.AddWithValue("@login", Login);
                        cmd.Parameters.AddWithValue("@password", Password1);

                        await cmd.ExecuteNonQueryAsync();

                        var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, $"{Login}"),
                        new Claim(ClaimTypes.UserData, $"{UserImageSrc}"),
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

                        return RedirectToPage("/UserProfile", new { login = Login });
                    }
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    return Page();
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
        }

        public async Task<IActionResult> OnPostAuthorization(string Login, string Password1)
        {
            if (!ModelState.IsValid)
                return Page();
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
                            if (Login != reader.GetString(0) && Password1 != reader.GetString(1))
                            {
                                ErrorMessage = "Неправильный логин или пароль";
                                return Page();
                            }
                            UserImageSrc = reader.GetString(3);
                        }
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, $"{Login}"),
                        new Claim(ClaimTypes.UserData, $"{UserImageSrc}"),
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
                    return RedirectToPage("/UserProfile", new { login = Login });
                }
                catch (Exception ex)
                {
                    ErrorMessage = ex.Message;
                    return Page();
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