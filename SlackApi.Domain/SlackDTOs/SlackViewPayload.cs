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

        [JsonPropertyName("view_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ViewId { get; set; }

        [JsonPropertyName("view")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
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

        [JsonPropertyName("clear_on_close")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? ClearOnClose { get; set; }

        [JsonPropertyName("notify_on_close")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? NotifyOnClose { get; set; }

        // Api Response Properties - Don't need setting
        [JsonPropertyName("id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ViewId { get; set; }

        [JsonPropertyName("team_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? TeamId { get; set; }

        [JsonPropertyName("previous_view_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? PreviousViewId { get; set; }

        [JsonPropertyName("root_view_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RootViewId { get; set; }

        [JsonPropertyName("app_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AppId { get; set; }

        [JsonPropertyName("external_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? ExternalId { get; set; }

        [JsonPropertyName("app_installed_team_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? AppInstalledTeamId { get; set; }

        [JsonPropertyName("bot_id")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? BotId { get; set; }
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

        [JsonPropertyName("emoji")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Emoji { get; set; }
    }
}
