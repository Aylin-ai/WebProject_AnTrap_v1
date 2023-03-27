using System;
using Newtonsoft.Json;
using WebVersion.AdditionalClasses.Bases;

namespace WebVersion.AdditionalClasses.Classes
{
    public class Anime : AnimeMangaBase
    {
        [JsonProperty("episodes")] public long Episodes { get; set; }
        [JsonProperty("episodes_aired")] public long EpisodesAired { get; set; }
    }
}