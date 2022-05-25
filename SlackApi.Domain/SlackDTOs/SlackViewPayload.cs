using System.Text.Json.Serialization;

namespace SlackApi.Domain.SlackDTOs
{
    public interface ISlackRequest
    {

    }

    public class SlackViewRequest : ISlackRequest
    {
        [JsonPropertyName("trigger_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TriggerId { get; set; }

        [JsonPropertyName("view")]
        public SlackViewPayload? View { get; set; }
    }

    /// <summary>
    /// https://api.slack.com/reference/surfaces/views
    /// </summary>
    public class SlackViewPayload
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "modal";

        [JsonPropertyName("title")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Title? Title { get; set; }

        [JsonPropertyName("blocks")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<Block>? Blocks { get; set; }

        [JsonPropertyName("close")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Close? Close { get; set; }

        [JsonPropertyName("submit")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Submit? Submit { get; set; }

        [JsonPropertyName("private_metadata")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PrivateMetadata { get; set; }

        [JsonPropertyName("callback_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? CallbackId { get; set; }
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
