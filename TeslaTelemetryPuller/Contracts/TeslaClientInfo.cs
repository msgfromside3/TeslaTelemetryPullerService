namespace TeslaTelemetryPuller.Contracts
{
    using Newtonsoft.Json;

    [JsonObject(MemberSerialization.OptIn)]
    public class TeslaClientInfo
    {
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        public class ApiV1
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("secret")]
            public string Secret { get; set; }

            [JsonProperty("baseurl")]
            public string BaseUrl { get; set; }

            [JsonProperty("api")]
            public string Api { get; set; }
        }

        [JsonProperty("v1")]
        public ApiV1 V1;
    }
}
