using Newtonsoft.Json;
using System.Collections.Generic;

namespace AutoPrintr.Core.Models
{
    public class User : BaseModel
    {
        [JsonProperty("user_token")]
        public string Token { get; set; }

        [JsonProperty("user_name")]
        public string UserName { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("domain")]
        public string Domain { get; set; }

        [JsonProperty("subdomain")]
        public string Subdomain { get; set; }

        [JsonProperty("default_location")]
        public int? DefaulLocationId { get; set; }

        [JsonProperty("enable_multi_locations")]
        public bool MultiLocationsAllowed { get; set; }

        [JsonProperty("locations_allowed")]
        public IEnumerable<Location> Locations { get; set; }
    }
}