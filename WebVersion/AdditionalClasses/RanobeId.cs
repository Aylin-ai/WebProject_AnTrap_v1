using Newtonsoft.Json;
using ShikimoriSharp.Classes;

namespace WebVersion.AdditionalClasses
{
    public class RanobeId : RanobeIdBase
    {
        [JsonProperty("licensors")]
        public string[] Licensors { get; set; }

        [JsonProperty("publishers")]
        public Publisher[] Publishers { get; set; }

        [JsonProperty("volumes")]
        public long Volumes { get; set; }

        [JsonProperty("chapters")]
        public long Chapters { get; set; }
    }
}
