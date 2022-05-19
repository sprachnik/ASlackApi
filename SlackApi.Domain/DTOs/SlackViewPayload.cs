using System.Text.Json.Serialization;

namespace SlackApi.Domain.DTOs
{
    /// <summary>
    /// https://api.slack.com/reference/surfaces/views
    /// </summary>
    public class SlackViewPayload
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "modal";

        [JsonPropertyName("title")]
        public Title? Title { get; set; }

        [JsonPropertyName("blocks")]
        public List<Block>? Blocks { get; set; }

        [JsonPropertyName("close")]
        public Close? Close { get; set; }

        [JsonPropertyName("submit")]
        public Submit? Submit { get; set; }

        [JsonPropertyName("private_metadata")]
        public string? PrivateMetadata { get; set; }

        [JsonPropertyName("callback_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CallbackId { get; set; }

        [JsonPropertyName("trigger_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TriggerId { get; set; }
    }

    public class Close
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    public class Submit
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    public class Title
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
