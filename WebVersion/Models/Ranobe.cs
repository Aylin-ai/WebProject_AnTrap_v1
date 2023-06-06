using Newtonsoft.Json;

namespace WebVersion.Models
{
    public class Ranobe : RanobeBase
    {
        [JsonProperty("volumes")]
        public int Volumes { get; set; }
        [JsonProperty("chapters")]
        public int Chapters { get; set; }
    }
}
