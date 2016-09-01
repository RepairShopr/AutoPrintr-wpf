using Newtonsoft.Json;

namespace AutoPrintr.Models
{
    public class Channel
    {
        [JsonProperty("messaging_channel")]
        public string Value { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}