using Newtonsoft.Json;

namespace WebVersion.AdditionalClasses
{
    public class Ranobe : RanobeBase
    {
        [JsonProperty("volumes")]
        public int Volumes { get; set; }
        [JsonProperty("chapters")]
        public int Chapters { get; set; }
    }
}
