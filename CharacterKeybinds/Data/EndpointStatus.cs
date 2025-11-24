using Newtonsoft.Json;

namespace flakysalt.CharacterKeybinds.Data
{
    public class EndpointStatus
    {
        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("duration")]
        public int Duration { get; set; }

        [JsonProperty("error")]
        public bool Error { get; set; }

        [JsonProperty("schemaValid")]
        public bool SchemaValid { get; set; }
    }
}
