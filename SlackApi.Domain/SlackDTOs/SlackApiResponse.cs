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

        [JsonPropertyName("user")]
        public SlackUserDTO? User { get; set; }

        [JsonPropertyName("cache_ts")]
        public int? CacheTs { get; set; }

        [JsonPropertyName("members")]
        public List<SlackUserDTO>? Members { get; set; }
    }

    public class ResponseMetadata
    {
        [JsonPropertyName("messages")]
        public List<string>? Messages { get; set; }

        [JsonPropertyName("response_metadata")]
        public Dictionary<string, string>? Metadata { get; set; }

        public string? NextCursor => Metadata != null && Metadata.ContainsKey("next_cursor") 
            ? Metadata["next_cursor"] : null;
    }
}
