using Newtonsoft.Json;
using WebVersion.AdditionalClasses.Bases;

namespace WebVersion.AdditionalClasses.Classes
{
    public class Manga : AnimeMangaBase
    {
        [JsonProperty("volumes")] public long Volumes { get; set; }
        [JsonProperty("chapters")] public long Chapters { get; set; }
    }

    public class MangaID : AnimeMangaIdBase
    {
        [JsonProperty("volumes")] public long Volumes { get; set; }
        [JsonProperty("chapters")] public long Chapters { get; set; }
    }
}