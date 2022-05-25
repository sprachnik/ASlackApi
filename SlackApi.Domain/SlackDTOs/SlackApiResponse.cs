using System.Text.Json.Serialization;

namespace SlackApi.Domain.SlackDTOs
{
    public class SlackApiResponse
    {
        [JsonPropertyName("ok")]
        public bool? Ok { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("response_metadata")]
        public ResponseMetadata? ResponseMetadata { get; set; }

        [JsonPropertyName("view")]
        public SlackViewPayload? View { get; set; }
    }

    public class ResponseMetadata
    {
        [JsonPropertyName("messages")]
        public List<string>? Messages { get; set; }
    }
}
