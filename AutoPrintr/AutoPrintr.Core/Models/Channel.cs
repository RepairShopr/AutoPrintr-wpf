using Newtonsoft.Json;
using System.Collections.Generic;

namespace AutoPrintr.Core.Models
{
    public class Channel : BaseModel
    {
        [JsonProperty("messaging_channel")]
        public string Value { get; set; }

        [JsonProperty("registers")]
        public IEnumerable<Register> Registers { get; set; }

        [JsonProperty("error")]
        public string Error { get; set; }
    }
}