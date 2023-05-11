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
        private int _role;
        private string _userImage = "";

        public IndexModel(ILogger<IndexModel> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostRegistration(string Login, string Password1, string Password2, string Email)
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
                        "Login = @login or Email = @email;";

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.CommandText = sql;
                    cmd.Connection = conn;

                    cmd.Parameters.AddWithValue("@login", Login);
                    cmd.Parameters.AddWithValue("@email", Email);

                    var reader = cmd.ExecuteReader();
                    cmd.Parameters.Clear();

                    if (reader.HasRows)
                    {
                        ErrorMessage = "Пользователь с таким логином или email уже существует";
                        return Page();
                    }
                    else
                    {
                        await reader.CloseAsync();
                        sql = "insert into userinformation " +
                        "(Login, Pasword, Email, UserRole_Id) " +
                        "values (@login, @password, @email, 1);";

                        cmd = new MySqlCommand();
                        cmd.CommandText = sql;
                        cmd.Connection = conn;

                        cmd.Parameters.AddWithValue("@login", Login);
                        cmd.Parameters.AddWithValue("@password", Password1);
                        cmd.Parameters.AddWithValue("@email", Email);

                        await cmd.ExecuteNonQueryAsync();

                        var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, $"{Login}"),
                        new Claim(ClaimTypes.UserData, $"images/OldPif.jpg"),
                        new Claim(ClaimTypes.Role, "Пользователь")
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
                        "Login = @login";

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.CommandText = sql;
                    cmd.Connection = conn;

                    cmd.Parameters.AddWithValue("@login", Login);

                    var reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            if (Login != reader.GetString(1) && Password1 != reader.GetString(2))
                            {
                                ErrorMessage = "Неправильный логин или пароль";
                                return Page();
                            }
                            _userImage = reader.GetString(4);
                            _role = reader.GetInt32(5);
                        }
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, $"{Login}"),
                        new Claim(ClaimTypes.UserData, $"{_userImage}"),
                        new Claim(ClaimTypes.Role, _role == 1 ? "Пользователь" : "Разработчик")
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