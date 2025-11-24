using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace flakysalt.CharacterKeybinds.Data
{
    public class ApiStatusResponse
    {
        [JsonProperty("updated_at")]
        public DateTime UpdatedAt { get; set; }

        [JsonProperty("data")]
        public List<EndpointStatus> Data { get; set; }
    }
}
