using SlackApi.App.Builders;
using SlackApi.App.HttpClients;
using SlackApi.Domain.Constants;
using SlackApi.Domain.DTOs;

namespace SlackApi.App.Services
{
    public class BadgeService : IBadgeService
    {
        private readonly SlackApiClient _slackApiClient;

        public BadgeService(SlackApiClient slackApiClient)
        {
            _slackApiClient = slackApiClient;
        }

        public async Task<SlackResponse> SendABadge(SlackInteractionPayload payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            var view = GenerateSendABadgeView(payload?.CallbackId, payload?.TriggerId);
            var response = await _slackApiClient.SlackPostRequest(SlackEndpoints.ViewsOpenUrl, view);
            return new SlackResponse();
        }

        #region private methods

        private SlackViewRequest GenerateSendABadgeView(string? callbackId, string? triggerId)
        {
            if (string.IsNullOrWhiteSpace(callbackId))
                throw new ArgumentNullException(nameof(callbackId));

            if (string.IsNullOrWhiteSpace(triggerId))
                throw new ArgumentNullException(nameof(triggerId));

            var viewBuilder = new ViewBuilder(SlackResponsePayloadType.Modal, callbackId, triggerId)
                .AddTitle("Send a badge");

            viewBuilder.AddBlock(BlockType.Section, 
                blockId: "intro", 
                new Text 
                { 
                    Type = TextType.Markdown, 
                    BlockText = "*Welcome* to ~my~ Block Kit _modal_!" 
                },
                new Accessory
                {
                    Type = AccessoryType.Button,
                    Text = new Text
                    {
                        Type = TextType.PlainText,
                        BlockText = "A Button!"
                    },
                    ActionId = "button-identifier"
                });

            return viewBuilder.ConstructRequest();
        }

        #endregion
    }

    public interface IBadgeService
    {
        Task<SlackResponse> SendABadge(SlackInteractionPayload payload);
    }
}
