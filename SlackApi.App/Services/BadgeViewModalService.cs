using SlackApi.App.Builders;
using SlackApi.App.HttpClients;
using SlackApi.App.MemStore;
using SlackApi.Core.Exceptions;
using SlackApi.Domain.BadgeDTOs;
using SlackApi.Domain.Constants;
using SlackApi.Domain.SlackDTOs;
using SlackApi.Repository.Cache;
using SlackApi.Repository.TableStorage;

namespace SlackApi.App.Services
{
    /// <summary>
    /// https://api.slack.com/surfaces/modals/using#updating_response
    /// </summary>
    public class BadgeViewModalService : IBadgeViewModalService
    {
        private readonly SlackApiClient _slackApiClient;
        private readonly ITableStorageService _tableStorageService;
        private readonly ITableStorageMemStore _tableStorageMemStore;
        private readonly ICache _cache;
        private readonly string _placeHolderImageShrugUrl = "https://i.imgur.com/Zam8zGt.jpg";

        public BadgeViewModalService(SlackApiClient slackApiClient,
            ITableStorageService tableStorageService,
            ITableStorageMemStore tableStorageMemStore,
            ICache cache)
        {
            _slackApiClient = slackApiClient;
            _tableStorageService = tableStorageService;
            _tableStorageMemStore = tableStorageMemStore;
            _cache = cache;
        }

        public async Task<SlackResponse> OpenSendBadgeView(SlackInteractionPayload payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            var request = await GenerateSendABadgeView(payload?.CallbackId, payload?.TriggerId, _placeHolderImageShrugUrl);
            await _slackApiClient.SlackPostRequest(SlackEndpoints.ViewsOpenUrl, request);

            return new();
        }

        public async Task<SlackResponse> SubmitBadgeView(SlackInteractionPayload payload)
        {
            if (payload?.View?.RootViewId == null)
                throw new ArgumentNullException(nameof(payload));

            return new();
        }

        public async Task<SlackResponse> SelectUserAction(SlackInteractionPayload payload)
        {
            if (payload == null || payload?.View?.RootViewId == null)
                throw new ArgumentNullException(nameof(payload));

            var slackUserId = payload?.Actions?.FirstOrDefault(a => a.SelectedUser != null)?.SelectedUser;

            if (slackUserId == null)
                throw new ArgumentNullException(nameof(slackUserId));

            if (slackUserId.Equals(payload?.User?.Id))
            {

            }

            var response = await _slackApiClient.SlackGetRequest(SlackEndpoints.UsersInfoUrl, 
                new Dictionary<string, string?> { { "user", slackUserId } });

            if (response?.Ok != true)
                throw new BusinessException($"Unable to process actionId {UserSelectionActionId}");


            return new();
        }

        public async Task<SlackResponse> SelectBadgeAction(SlackInteractionPayload payload)
        {
            if (payload?.View?.RootViewId == null)
                throw new ArgumentNullException(nameof(payload));

            var selectedBadgeValue = payload.Actions?.Select(a => a.SelectedOption)?.FirstOrDefault()?.Value;

            if (selectedBadgeValue == null)
                throw new ArgumentNullException(nameof(selectedBadgeValue));

            var badges = await GetAllBadges();
            var selectedBadge = badges?.FirstOrDefault(b => b.Name == selectedBadgeValue);

            if (selectedBadge?.ImageUrl == null)
                throw new ArgumentNullException(nameof(selectedBadge));

            var request = await GenerateSendABadgeView(callbackId: null, triggerId: null, 
                selectedBadge.ImageUrl, selectedBadge.Name);
            
            if (request?.View == null)
                throw new ArgumentNullException(nameof(request));

            request.ViewId = payload?.View?.RootViewId;
            request.Hash = payload?.View?.Hash;
            
            await _slackApiClient.SlackPostRequest(SlackEndpoints.ViewsUpdateUrl, request);

            return new();
        }

        #region private methods

        private async Task<SlackViewRequest> GenerateSendABadgeView(string? callbackId, 
            string? triggerId,
            string? badgeImageUrl,
            string? badgeText = null)
        {
            return (SlackViewRequest)new ViewBuilder(SlackResponsePayloadType.Modal, callbackId, triggerId)
                .AddTitle("Send a badge")
                .AddAccessoryBlock(BlockType.Section,
                    blockId: "intro",
                    new Text
                    {
                        Type = TextType.Markdown,
                        BlockText = "*Select a funky badge for a colleague!*"
                    })
                .AddUsersSelectBlock("Select a user", UserSelectionActionId)
                .AddAccessoryBlock(BlockType.Section,
                    blockId: BadgeSelectBlockId,
                    new Text
                    {
                        Type = TextType.Markdown,
                        BlockText = "**Select a badge!**"
                    },
                    new Accessory
                    {
                        Type = AccessoryType.StaticSelect,
                        ActionId = BadgeSelectActionId,
                        Placeholder = new Placeholder
                        {
                            Type = TextType.PlainText,
                            Text = "Select an item"
                        },
                        Options = (await GetAllBadges()).Select(b =>
                            new Option
                            {
                                Text = new Text
                                {
                                    Type = TextType.PlainText,
                                    BlockText = b.Name
                                },
                                Value = b.PartitionKey
                            }).ToList()
                    })
                .AddImageBlock(blockId: BadgeImageBlockId,
                    imageUrl: badgeImageUrl ?? _placeHolderImageShrugUrl,
                    altText: badgeText ?? "Select a badge...",
                    new Title
                    {
                        Text = badgeText ?? "Select a badge...",
                        Type = TextType.PlainText
                    })
                .AddInputBlock(blockId: BadgeFeedbackBlockId,
                    new Element
                    {
                        Type = ElementType.PlainTextInput,
                        Multiline = true,
                        ActionId = BadgeFeedbackActionId,
                    },
                    new Label
                    {
                        Type = TextType.PlainText,
                        Text = "Feedback",
                        Emoji = true
                    })
                .AddSubmit(TextType.PlainText, "Submit")
                .ConstructRequest();
        }

        private async Task<List<BadgeTableEntity>> GetAllBadges()
        {
            var badges = await _tableStorageMemStore.Fetch<BadgeTableEntity>(TableStorageTable.Badges);
            return badges.Where(b => b.DateDeleted == null).ToList();
        }

        #endregion

        // Actions
        public const string UserSelectionActionId = "give-badge-select-user-action";
        public const string BadgeFeedbackActionId = "give-badge-feedback-action";
        public const string BadgeSelectActionId = "give-badge-select-action";

        // Blocks
        public const string BadgeImageBlockId = "give-badge-image-block";
        public const string BadgeFeedbackBlockId = "give-badge-feedback-block";
        public const string BadgeSelectBlockId = "give-badge-select-block";
    }

    public interface IBadgeViewModalService
    {
        Task<SlackResponse> OpenSendBadgeView(SlackInteractionPayload payload);
        Task<SlackResponse> SubmitBadgeView(SlackInteractionPayload payload);
        Task<SlackResponse> SelectBadgeAction(SlackInteractionPayload payload);
        Task<SlackResponse> SelectUserAction(SlackInteractionPayload payload);
    }
}
