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

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult UserProfile()
        {
            return View();
        }

        public IActionResult MainAnimePage()
        {
            ViewBag.Order = 1;
            return View();
        }

        //public async Task<IActionResult> AboutAnTrap()
        //{
        //    //var httpClient = _httpClientFactory.CreateClient();
        //    //httpClient.BaseAddress = new Uri("https://shikimori.one");
        //    //MangaID? manga = await httpClient.GetFromJsonAsync<MangaID>("/api/mangas/564");
        //    //return Content(manga?.Russian);
        //}

        public IActionResult Registration(string Login, string Password1, string Password2)
        {
            string registrationData = $"Login: {Login}, Password1: {Password1}, Password2: {Password2}";
            return Content(registrationData);
        }

        public IActionResult Authorization(string Login, string Password1)
        {
            string registrationData = $"Login: {Login}, Password: {Password1}";
            return Content(registrationData);
        }

    }
}
