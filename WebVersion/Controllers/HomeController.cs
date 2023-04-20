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

namespace WebVersion.Controllers
{
    public class HomeController : Controller
    {
        private IHttpClientFactory _httpClientFactory;
        private ILogger logger;

        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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
                    case "animeList":
                        return RedirectToPage("/UserList/AnimeList");
                    case "mangaList":
                        return RedirectToPage("/UserList/MangaList");
                    case "ranobeList":
                        return RedirectToPage("/UserList/RanobeList");
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
                        sql = "delete from anime " +
                            "where Anime_AnimeId = @animeId";
                    }
                    else
                    {
                        if (AnimeInList.Keys.Where(x => x == selectedList.Split(' ')[1]).Count() == 0)
                        {
                            sql = "insert into anime (Anime_AnimeId, Anime_UserInformation_Login, " +
                                "Anime_ListInformation_Id) values " +
                                "(@animeId, @user, @listId);";
                        }
                        else
                        {
                            sql = "update anime " +
                                "set Anime_ListInformation_Id = @listId " +
                                "where Anime_AnimeId = @animeId and " +
                                "Anime_UserInformation_Login = @user;";
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

        public async Task AddMangaToList(string selectedList)
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
                        sql = "delete from manga " +
                            "where Manga_MangaId = @mangaId";
                    }
                    else
                    {
                        if (MangaInList.Keys.Where(x => x == selectedList.Split(' ')[1]).Count() == 0)
                        {
                            sql = "insert into manga (Manga_MangaId, Manga_UserInformation_Login, " +
                                "Manga_ListInformation_Id) values " +
                                "(@mangaId, @user, @listId);";
                        }
                        else
                        {
                            sql = "update manga " +
                                "set Manga_ListInformation_Id = @listId " +
                                "where Manga_MangaId = @animeId and " +
                                "Manga_UserInformation_Login = @user;";
                        }
                    }

                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@mangaId", selectedList.Split(" ")[1]);
                    cmd.Parameters.AddWithValue("@user", User.Identity.Name);
                    cmd.Parameters.AddWithValue("@listId", selectedList.Split(" ")[0]);

                    await cmd.ExecuteNonQueryAsync();
                    MangaInList.Clear();
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
            else
                RedirectToPage("/Index");
        }

        public async Task AddRanobeToList(string selectedList)
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
                        sql = "delete from ranobe " +
                            "where Ranobe_RanobeId = @ranobeId";
                    }
                    else
                    {
                        if (RanobeInList.Keys.Where(x => x == selectedList.Split(' ')[1]).Count() == 0)
                        {
                            sql = "insert into ranobe (Ranobe_RanobeId, Ranobe_UserInformation_Login, " +
                                "Ranobe_ListInformation_Id) values " +
                                "(@ranobeId, @user, @listId);";
                        }
                        else
                        {
                            sql = "update ranobe " +
                                "set Ranobe_ListInformation_Id = @listId " +
                                "where Ranobe_RanobeId = @ranobeId and " +
                                "Ranobe_UserInformation_Login = @user;";
                        }
                    }

                    cmd.CommandText = sql;
                    cmd.Parameters.AddWithValue("@ranobeId", selectedList.Split(" ")[1]);
                    cmd.Parameters.AddWithValue("@user", User.Identity.Name);
                    cmd.Parameters.AddWithValue("@listId", selectedList.Split(" ")[0]);

                    await cmd.ExecuteNonQueryAsync();
                    RanobeInList.Clear();
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
            else
                RedirectToPage("/Index");
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

                string sql = "select * from anime " +
                    "where Anime_UserInformation_Login = @login";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@login", User.Identity.Name);

                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        temporaryDic[reader.GetInt32(1).ToString()] = reader.GetInt32(3).ToString();
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

                string sql = "select * from manga" +
                    "where Manga_UserInformation_Login = @login";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@login", User.Identity.Name);

                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        temporaryDic[reader.GetInt32(1).ToString()] = reader.GetInt32(3).ToString();
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

                string sql = "select * from ranobe" +
                    "where Ranobe_UserInformation_Login = @login";
                cmd.CommandText = sql;
                cmd.Parameters.AddWithValue("@login", User.Identity.Name);

                var reader = cmd.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        temporaryDic[reader.GetInt32(1).ToString()] = reader.GetInt32(3).ToString();
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
