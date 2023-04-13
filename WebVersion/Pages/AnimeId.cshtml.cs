using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using ShikimoriSharp.Bases;
using ShikimoriSharp;
using WebVersion.Models;
using ShikimoriSharp.Classes;
using ShikimoriSharp.AdditionalRequests;

namespace WebVersion.Pages
{
    public class AnimeIdModel : PageModel
    {
        private ILogger logger;

        [BindProperty(Name = "animeId", SupportsGet = true)]
        public int AnimeId { get; set; }
        public AnimeID? Anime { get; set; }
        public Related?[] RelatedAnime { get; set; }
        public Anime[] SimilarAnime { get; set; }

        public AnimeIdModel()
        {

        }

        public async Task<IActionResult> OnGetAsync()
        {
            var client = new ShikimoriClient(logger, new ClientSettings("ShikiOAuthTest", APIToken.clientID, APIToken.clientSecret));
            Anime = await client.Animes.GetAnime(AnimeId);
            RelatedAnime = await client.Animes.GetRelated(AnimeId);
            SimilarAnime = await client.Animes.GetSimilar(AnimeId);
            if (Anime == null)
            {
                return NotFound();
            }
            return Page();
        }
    }
}
