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
                    case "userList":
                        return RedirectToPage("/MainAnimePage");
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


        //public async Task<IActionResult> AboutAnTrap()
        //{
        //    //var httpClient = _httpClientFactory.CreateClient();
        //    //httpClient.BaseAddress = new Uri("https://shikimori.one");
        //    //MangaID? manga = await httpClient.GetFromJsonAsync<MangaID>("/api/mangas/564");
        //    //return Content(manga?.Russian);
        //}

    }
}
