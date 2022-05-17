//using Microsoft.AspNetCore.Mvc;
//using System.Text.Json.Serialization;

//namespace SlackApi.Domain.DTOs
//{
//    public class User
//    {
//        [JsonPropertyName("id")]
//        [FromForm(Name = "id")]
//        public string? UserId { get; set; }

//        [JsonPropertyName("username")]
//        [FromForm(Name = "username")]
//        public string? Username { get; set; }

//        [JsonPropertyName("name")]
//        [FromForm(Name = "name")]
//        public string? Name { get; set; }

//        [JsonPropertyName("team_id")]
//        [FromForm(Name = "team_id")]
//        public string? TeamId { get; set; }
//    }

//    public class Container
//    {
//        [JsonPropertyName("type")]
//        [FromForm(Name = "type")]
//        public string? Type { get; set; }

//        [JsonPropertyName("message_ts")]
//        [FromForm(Name = "message_ts")]
//        public string? MessageTs { get; set; }

//        [JsonPropertyName("channel_id")]
//        [FromForm(Name = "channel_id")]
//        public string? Channel { get; set; }

//        [JsonPropertyName("is_ephemeral")]
//        [FromForm(Name = "is_ephemeral")]
//        public bool IsEphemeral { get; set; }
//    }

//    public class Team
//    {
//        [JsonPropertyName("id")]
//        [FromForm(Name = "id")]
//        public string? TeamId { get; set; }

//        [JsonPropertyName("domain")]
//        [FromForm(Name = "domain")]
//        public string? Domain { get; set; }
//    }

//    public class Channel
//    {
//        [JsonPropertyName("id")]
//        [FromForm(Name = "id")]
//        public string? ChannelId { get; set; }

//        [JsonPropertyName("name")]
//        [FromForm(Name = "name")]
//        public string? Name { get; set; }
//    }

//    public class TextBlock
//    {
//        [JsonPropertyName("type")]
//        [FromForm(Name = "type")]
//        public string? Type { get; set; }

//        [JsonPropertyName("text")]
//        [FromForm(Name = "text")]
//        public string? Text { get; set; }

//        [JsonPropertyName("emoji")]
//        [FromForm(Name = "emoji")]
//        public bool Emoji { get; set; }
//    }

//    public class Accessory
//    {
//        [JsonPropertyName("type")]
//        [FromForm(Name = "type")]
//        public string? Type { get; set; }

//        [JsonPropertyName("action_id")]
//        [FromForm(Name = "action_id")]
//        public string? ActionId { get; set; }

//        [JsonPropertyName("text")]
//        [FromForm(Name = "text")]
//        public TextBlock? Text { get; set; }

//        [JsonPropertyName("value")]
//        [FromForm(Name = "value")]
//        public string? Value { get; set; }
//    }

//    public class Block
//    {
//        [JsonPropertyName("type")]
//        [FromForm(Name = "type")]
//        public string? Type { get; set; }

//        [JsonPropertyName("block_id")]
//        [FromForm(Name = "block_id")]
//        public string? BlockId { get; set; }

//        [JsonPropertyName("text")]
//        [FromForm(Name = "text")]
//        public TextBlock? Text { get; set; }

//        [JsonPropertyName("accessory")]
//        [FromForm(Name = "accessory")]
//        public Accessory? Accessory { get; set; }
//    }

//    public class Message
//    {
//        [JsonPropertyName("bot_id")]
//        [FromForm(Name = "bot_id")]
//        public string? BotId { get; set; }

//        [JsonPropertyName("type")]
//        [FromForm(Name = "type")]
//        public string? Type { get; set; }

//        [JsonPropertyName("text")]
//        [FromForm(Name = "text")]
//        public string? Text { get; set; }

//        [JsonPropertyName("user")]
//        [FromForm(Name = "user")]
//        public string? User { get; set; }

//        [JsonPropertyName("ts")]
//        [FromForm(Name = "ts")]
//        public string? Ts { get; set; }

//        [JsonPropertyName("team")]
//        [FromForm(Name = "team")]
//        public string? Team { get; set; }

//        [JsonPropertyName("blocks")]
//        [FromForm(Name = "blocks")]
//        public List<Block>? Blocks { get; set; }
//    }

//    public class State
//    {
//        [JsonPropertyName("values")]
//        [FromForm(Name = "values")]
//        public Dictionary<string, string>? Values { get; set; }
//    }

//    public class Action
//    {
//        [JsonPropertyName("action_id")]
//        [FromForm(Name = "action_id")]
//        public string? ActionId { get; set; }

//        [JsonPropertyName("block_id")]
//        [FromForm(Name = "block_id")]
//        public string? BlockId { get; set; }

//        [JsonPropertyName("text")]
//        [FromForm(Name = "text")]
//        public TextBlock? Text { get; set; }

//        [JsonPropertyName("value")]
//        [FromForm(Name = "value")]
//        public string? Value { get; set; }

//        [JsonPropertyName("type")]
//        [FromForm(Name = "type")]
//        public string? Type { get; set; }

//        [JsonPropertyName("action_ts")]
//        [FromForm(Name = "action_ts")]
//        public double ActionTime { get; set; }

//        [JsonPropertyName("selected_option")]
//        [FromForm(Name = "selected_option")]
//        public SelectedOption? SelectedOption { get; set; }

//        [JsonIgnore]
//        public DateTimeOffset ActionDate => DateTimeOffset.FromUnixTimeSeconds((long)ActionTime);
//    }

//    public class SelectedOption
//    {
//        [JsonPropertyName("text")]
//        [FromForm(Name = "text")]
//        public TextBlock? Text { get; set; }

//        [JsonPropertyName("value")]
//        [FromForm(Name = "value")]
//        public string? Value { get; set; }
//    }

//    public class SlackInteractiveEvent
//    {
//        [JsonPropertyName("type")]
//        [FromForm(Name = "type")]
//        public string? Type { get; set; }

//        [JsonPropertyName("user")]
//        [FromForm(Name = "user")]
//        public User? User { get; set; }

//        [JsonPropertyName("api_app_id")]
//        [FromForm(Name = "api_app_id")]
//        public string? ApiAppId { get; set; }

//        [JsonPropertyName("token")]
//        [FromForm(Name = "token")]
//        public string? Token { get; set; }

//        [JsonPropertyName("container")]
//        [FromForm(Name = "container")]
//        public Container? Container { get; set; }

//        [JsonPropertyName("trigger_id")]
//        [FromForm(Name = "trigger_id")]
//        public string? TriggerId { get; set; }

//        [JsonPropertyName("team")]
//        [FromForm(Name = "team")]
//        public Team? Team { get; set; }

//        [JsonPropertyName("enterprise")]
//        [FromForm(Name = "enterprise")]
//        public object? Enterprise { get; set; }

//        [JsonPropertyName("is_enterprise_install")]
//        [FromForm(Name = "is_enterprise_install")]
//        public bool IsEnterpriseInstall { get; set; }

//        [JsonPropertyName("channel")]
//        [FromForm(Name = "channel")]
//        public Channel? Channel { get; set; }

//        [JsonPropertyName("message")]
//        [FromForm(Name = "message")]
//        public Message? Message { get; set; }

//        [JsonPropertyName("response_url")]
//        [FromForm(Name = "response_url")]
//        public string? ResponseUrl { get; set; }

//        [JsonPropertyName("actions")]
//        [FromForm(Name = "actions")]
//        public List<Action>? BlockActions { get; set; }
//    }
//}
