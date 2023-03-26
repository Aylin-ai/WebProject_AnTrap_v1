using System;
using Newtonsoft.Json;

namespace WebVersion.AdditionalClasses.Classes
{
    public class Calendar
    {
        [JsonProperty("next_episode")] public long NextEpisode { get; set; }
        [JsonProperty("next_episode_at")] public DateTimeOffset NextEpisodeAt { get; set; }
        [JsonProperty("duration")] public string Duration { get; set; }
        [JsonProperty("anime")] public Anime Anime { get; set; }
    }
}