using System.Text.Json.Serialization;

namespace SlackApi.Domain.DTOs
{
    public static class SlackEventType
    {
        public static readonly string AppMention = "app_mention";
        public static readonly string Message = "message";
    }

    public static class SlackChannelType
    {
        public static readonly string Channel = "channel";
    }

    public class Event
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("user")]
        public string? UserId { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("event_ts")]
        public double EventTime { get; set; }

        public DateTimeOffset EventDate => DateTimeOffset.FromUnixTimeSeconds((long)EventTime);

        [JsonPropertyName("channel_type")]
        public string? ChannelType { get; set; }

        [JsonPropertyName("channel")]
        public string? Channel { get; set; }
    }

    /// <summary>
    /// https://api.slack.com/apis/connections/events-api
    /// </summary>
    public class SlackEvent
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("challenge")]
        public string? Challenge { get; set; }

        [JsonPropertyName("team_id")]
        public string? TeamId { get; set; }

        [JsonPropertyName("api_app_id")]
        public string? ApiAppId { get; set; }

        [JsonPropertyName("event")]
        public Event? Event { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("authed_users")]
        public List<string>? AuthedUsers { get; set; }

        [JsonPropertyName("authed_teams")]
        public List<string>? AuthedTeams { get; set; }

        [JsonPropertyName("event_id")]
        public string? EventId { get; set; }

        [JsonPropertyName("event_time")]
        public int EventTime { get; set; }

        public DateTimeOffset EventDate => DateTimeOffset.FromUnixTimeSeconds(EventTime);
    }

    public class SlackResponse
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        [JsonPropertyName("challenge")]
        public string? Challenge { get; set; }
    }
}
