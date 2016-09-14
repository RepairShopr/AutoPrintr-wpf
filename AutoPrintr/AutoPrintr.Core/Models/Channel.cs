using Newtonsoft.Json;

namespace AutoPrintr.Core.Models
{
    public class Channel : BaseModel
    {
        [JsonProperty("messaging_channel")]
        public string Value { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}