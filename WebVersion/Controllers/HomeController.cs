using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using MySql.Data.MySqlClient;
using System.Data.Common;
using System.Drawing;
using System.Net.Http;
using System.Runtime;
using System.Text.Json;
using ShikimoriSharp.Bases;
using ShikimoriSharp.Classes;
using ShikimoriSharp;
using WebVersion.Models;
using WebVersion.Pages;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using WebVersion.AdditionalClasses;
using Microsoft.AspNetCore.SignalR;

namespace WebVersion.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private IHttpClientFactory _httpClientFactory;
        private ILogger logger;

        public HomeController(IHttpClientFactory httpClientFactory, IHubContext<NotificationHub> hubContext)
        {
            _httpClientFactory = httpClientFactory;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> SendNotify(int userId, string message = "Уведомление")
        {
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", message);
            return Redirect(Request.Headers["Referer"].ToString());
        }


        public IActionResult Selected(string selectedValue)
        {
            if (User.Identity.IsAuthenticated)
            {
                switch (selectedValue)
                {
                    case "anime":
                        return RedirectToPage("/MainAnimePage");
                    case "manga":
                        return RedirectToPage("/Manga");
                    case "ranobe":
                        return RedirectToPage("/Ranobe");
                    default:
                        return RedirectToPage("/Error");
                }
            }
            else
                return RedirectToPage("/Index");
        }

        public async Task<IActionResult> SelectionFromUser(string selectedValue)
        {
            if (User.Identity.IsAuthenticated)
            {
                switch (selectedValue)
                {
                    case "userList":
                        return RedirectToPage("/UserList");
                    case "animeList":
                        return RedirectToPage("/AnimeList");
                    case "mangaList":
                        return RedirectToPage("/MangaList");
                    case "ranobeList":
                        return RedirectToPage("/RanobeList");
                    case "settings":
                        return RedirectToPage("/UserProfile");
                    case "logout":
                        await HttpContext.SignOutAsync("MyCookieAuthenticationScheme");
                        return RedirectToPage("/Index");
                    default:
                        return RedirectToPage("/Error");
                }
            }
            else
                return RedirectToPage("/Index");
        }

        public async Task<IActionResult> AddAnimeToList(string selectedList)
        {
            if (User.Identity.IsAuthenticated)
            {
                var AnimeInList = GetAnimesFromUserList();
                MySqlConnection conn = DBUtils.GetDBConnection();
                conn.Open();

                try
                {
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;

                    string sql = "";

                    if (selectedList.Split(" ")[0] == "0")
                    {
                        sql = "delete from piece " +
                            "where PieceId = @animeId and " +
                            "Kind = 'аниме'";
                    }
                    else
                    {
                        if (!AnimeInList.Keys.Any(x => x == selectedList.Split(' ')[1]))
                        {
                            sql = "insert into piece (Kind, PieceId, UserInformation_Login, " +
                                "ListInformation_Id) values " +
                                "('аниме', @animeId, @user, @listId);";
                        }
                        else
                        {
                            sql = "update piece " +
                                "set ListInformation_Id = @listId " +
                                "where PieceId = @animeId and " +
                                "UserInformation_Login = @user " +
                                "and Kind = 'аниме';";
                        }
                    }

                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@animeId", selectedList.Split(" ")[1]);
                    cmd.Parameters.AddWithValue("@user", User.Identity.Name);
                    cmd.Parameters.AddWithValue("@listId", selectedList.Split(" ")[0]);

                    await cmd.ExecuteNonQueryAsync();
                    AnimeInList.Clear();
                    return Redirect(Request.Headers["Referer"].ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return RedirectToPage("/Error");
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }

            }
            else
                return RedirectToPage("/Index");
        }

        public async Task<IActionResult> AddMangaToList(string selectedList)
        {
            if (User.Identity.IsAuthenticated)
            {
                var MangaInList = GetMangasFromUserList();

                MySqlConnection conn = DBUtils.GetDBConnection();
                conn.Open();

                try
                {
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;

                    string sql = "";

                    if (selectedList.Split(" ")[0] == "0")
                    {
                        sql = "delete from piece " +
                            "where PieceId = @mangaId and " +
                            "Kind = 'манга'";
                    }
                    else
                    {
                        if (!MangaInList.Keys.Any(x => x == selectedList.Split(' ')[1]))
                        {
                            sql = "insert into piece (Kind, PieceId, UserInformation_Login, " +
                                "ListInformation_Id) values " +
                                "('манга', @mangaId, @user, @listId);";
                        }
                        else
                        {
                            sql = "update piece " +
                                "set ListInformation_Id = @listId " +
                                "where PieceId = @mangaId and " +
                                "UserInformation_Login = @user " +
                                "and Kind = 'манга';";
                        }
                    }

                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@mangaId", selectedList.Split(" ")[1]);
                    cmd.Parameters.AddWithValue("@user", User.Identity.Name);
                    cmd.Parameters.AddWithValue("@listId", selectedList.Split(" ")[0]);

                    await cmd.ExecuteNonQueryAsync();
                    MangaInList.Clear();
                    return Redirect(Request.Headers["Referer"].ToString());
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return RedirectToPage("/Error");
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            else
                return RedirectToPage("/Index");
        }

        public async Task<IActionResult> AddRanobeToList(string selectedList)
        {
            if (User.Identity.IsAuthenticated)
            {
                var RanobeInList = GetRanobeFromUserList();

                MySqlConnection conn = DBUtils.GetDBConnection();
                conn.Open();

                try
                {
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;

                    string sql = "";

                    if (selectedList.Split(" ")[0] == "0")
                    {
                        sql = "delete from piece " +
                            "where PieceId = @ranobeId and " +
                            "Kind = 'ранобэ'";
                    }
                    else
                    {
                        if (!RanobeInList.Keys.Any(x => x == selectedList.Split(' ')[1]))
                        {
                            sql = "insert into piece (Kind, PieceId, UserInformation_Login, " +
                                "ListInformation_Id) values " +
                                "('ранобэ', @ranobeId, @user, @listId);";
                        }
                        else
                        {
                            sql = "update piece " +
                                "set ListInformation_Id = @listId " +
                                "where PieceId = @ranobeId and " +
                                "serInformation_Login = @user " +
                                "and Kind = 'ранобэ';";
                        }
                    }

                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@ranobeId", selectedList.Split(" ")[1]);
                    cmd.Parameters.AddWithValue("@user", User.Identity.Name);
                    cmd.Parameters.AddWithValue("@listId", selectedList.Split(" ")[0]);

                    await cmd.ExecuteNonQueryAsync();
                    RanobeInList.Clear();
                    return Redirect(Request.Headers["Referer"].ToString());
                }
                catch (Exception ex)
                {
                    return RedirectToPage("/Error");
                }
                finally
                {
                    conn.Close();
                    conn.Dispose();
                }
            }
            else
                return RedirectToPage("/Index");
        }


        public Dictionary<string, string> GetAnimesFromUserList()
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            Dictionary<string, string> temporaryDic = new Dictionary<string, string>();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select * from piece " +
                    "where UserInformation_Login = @login " +
                    "and Kind = 'аниме';";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@login", User.Identity.Name);

                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        temporaryDic[reader.GetInt32(2).ToString()] = reader.GetInt32(4).ToString();
                    }
                }
                return temporaryDic;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Dictionary<string, string>();
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        public Dictionary<string, string> GetMangasFromUserList()
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();

            Dictionary<string, string> temporaryDic = new Dictionary<string, string>();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select * from piece " +
                    "where UserInformation_Login = @login " +
                    "and Kind = 'манга';";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@login", User.Identity.Name);

                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        temporaryDic[reader.GetInt32(2).ToString()] = reader.GetInt32(4).ToString();
                    }
                }
                return temporaryDic;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Dictionary<string, string>();
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }

        public Dictionary<string, string> GetRanobeFromUserList()
        {
            MySqlConnection conn = DBUtils.GetDBConnection();
            conn.Open();
            Dictionary<string, string> temporaryDic = new Dictionary<string, string>();

            try
            {
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;

                string sql = "select * from piece " +
                    "where UserInformation_Login = @login " +
                    "and Kind = 'ранобэ';";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@login", User.Identity.Name);

                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        temporaryDic[reader.GetInt32(2).ToString()] = reader.GetInt32(4).ToString();
                    }
                }
                return temporaryDic;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new Dictionary<string, string>();
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }
    }
}
