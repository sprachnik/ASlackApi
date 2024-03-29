﻿using System.Text.Json.Serialization;

namespace SlackApi.Domain.SlackDTOs
{

    public class SlackInteractionPayload
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("team")]
        public Team? Team { get; set; }

        [JsonPropertyName("user")]
        public SlackUserDTO? User { get; set; }

        [JsonPropertyName("api_app_id")]
        public string? ApiAppId { get; set; }

        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("container")]
        public Container? Container { get; set; }

        [JsonPropertyName("trigger_id")]
        public string? TriggerId { get; set; }

        [JsonPropertyName("callback_id")]
        public string? CallbackId { get; set; }

        [JsonPropertyName("channel")]
        public Channel? Channel { get; set; }

        [JsonPropertyName("message")]
        public Message? Message { get; set; }

        [JsonPropertyName("response_url")]
        public string? ResponseUrl { get; set; }

        [JsonPropertyName("view")]
        public View? View { get; set; }

        [JsonPropertyName("actions")]
        public List<Action>? Actions { get; set; }

        [JsonPropertyName("is_cleared")]
        public bool? IsCleared { get; set; }


        public List<BlockActionValue> GetBlockActionValues(List<BlockStateValueValidator> validators)
        {
            var blockActionValues = new List<BlockActionValue>();

            if (View?.State?.Values == null)
                return blockActionValues;

            foreach (var validator in validators)
            {
                string? value = null;

                var (blockId, actions) = View.State.Values.FirstOrDefault(s => s.Key == validator.BlockId);

                if (actions != null)
                {
                    value = actions
                        .Select(a => (a.Value?.Value) 
                            ?? (a.Value?.SelectedOption?.Value != null ? a.Value?.SelectedOption?.Value : null))
                        .FirstOrDefault();
                }

                blockActionValues.Add(new(validator.BlockId, validator.BlockType, value, validator.IsValid(value), validator.ValidationMessage));
            }

            return blockActionValues;
        }
    }

    public record BlockActionValue(string BlockId, string BlockType, string? Value, bool IsValid, string ValidationMessage);

    public record BlockStateValueValidator(string BlockId, Func<string?,bool> Validator, string ValidationMessage, string BlockType)
    {
        public bool IsValid(string? stateValue) => Validator(stateValue);
    }

    public class ViewState
    {
        [JsonPropertyName("values")]
        public Dictionary<string, Dictionary<string, StateValue>>? Values { get; set; }
    }

    public class Values
    {
        [JsonPropertyName("multiline")]
        public Multiline? Multiline { get; set; }

        [JsonPropertyName("target_channel")]
        public TargetChannel? TargetChannel { get; set; }
    }

    public class Mlvalue
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }

    public class Multiline
    {
        [JsonPropertyName("mlvalue")]
        public Mlvalue? Mlvalue { get; set; }
    }

    public class ResponseUrl
    {
        [JsonPropertyName("block_id")]
        public string? BlockId { get; set; }

        [JsonPropertyName("action_id")]
        public string? ActionId { get; set; }

        [JsonPropertyName("channel_id")]
        public string? ChannelId { get; set; }

        [JsonPropertyName("response_url")]
        public string? CurrentResponseUrl { get; set; }
    }

    public class TargetChannel
    {
        [JsonPropertyName("target_select")]
        public TargetSelect? TargetSelect { get; set; }
    }

    public class TargetSelect
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("selected_conversation")]
        public string? SelectedConversation { get; set; }
    }

    public class Action
    {
        [JsonPropertyName("action_id")]
        public string? ActionId { get; set; }

        [JsonPropertyName("block_id")]
        public string? BlockId { get; set; }

        [JsonPropertyName("text")]
        public Text? Text { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("action_ts")]
        public string? ActionTs { get; set; }

        [JsonPropertyName("selected_user")]
        public string? SelectedUser { get; set; }

        [JsonPropertyName("selected_option")]
        public SelectedOption? SelectedOption { get; set; }
    }

    public class StateValue
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("selected_option")]
        public SelectedOption? SelectedOption { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }

    public class SelectedOption
    {
        [JsonPropertyName("text")]
        public Text? Text { get; set; }

        [JsonPropertyName("value")]
        public string? Value { get; set; }
    }


    public class Channel
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }

    public class Container
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("message_ts")]
        public string? MessageTs { get; set; }

        [JsonPropertyName("attachment_id")]
        public int AttachmentId { get; set; }

        [JsonPropertyName("channel_id")]
        public string? ChannelId { get; set; }

        [JsonPropertyName("is_ephemeral")]
        public bool IsEphemeral { get; set; }

        [JsonPropertyName("is_app_unfurl")]
        public bool IsAppUnfurl { get; set; }

        [JsonPropertyName("view_id")]
        public string? ViewId { get; set; }
    }

    public class Message
    {
        [JsonPropertyName("bot_id")]
        public string? BotId { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("user")]
        public string? User { get; set; }

        [JsonPropertyName("ts")]
        public string? Ts { get; set; }
    }

    public class Team
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("domain")]
        public string? Domain { get; set; }
    }

    public class View
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("team_id")]
        public string? TeamId { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("blocks")]
        public List<Block>? Blocks { get; set; }

        [JsonPropertyName("private_metadata")]
        public string? PrivateMetadata { get; set; }

        [JsonPropertyName("callback_id")]
        public string? CallbackId { get; set; }

        [JsonPropertyName("state")]
        public ViewState? State { get; set; }

        [JsonPropertyName("hash")]
        public string? Hash { get; set; }

        [JsonPropertyName("clear_on_close")]
        public bool ClearOnClose { get; set; }

        [JsonPropertyName("notify_on_close")]
        public bool NotifyOnClose { get; set; }

        [JsonPropertyName("close")]
        public object? Close { get; set; }

        [JsonPropertyName("submit")]
        public object? Submit { get; set; }

        [JsonPropertyName("previous_view_id")]
        public object? PreviousViewId { get; set; }

        [JsonPropertyName("root_view_id")]
        public string? RootViewId { get; set; }

        [JsonPropertyName("app_id")]
        public string? AppId { get; set; }

        [JsonPropertyName("external_id")]
        public string? ExternalId { get; set; }

        [JsonPropertyName("app_installed_team_id")]
        public string? AppInstalledTeamId { get; set; }

        [JsonPropertyName("bot_id")]
        public string? BotId { get; set; }

        [JsonPropertyName("response_urls")]
        public List<ResponseUrl>? ResponseUrls { get; set; }
    }
}
