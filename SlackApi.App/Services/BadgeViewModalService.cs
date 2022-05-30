using SlackApi.App.Builders;
using SlackApi.App.HttpClients;
using SlackApi.App.MemStore;
using SlackApi.Domain.BadgeDTOs;
using SlackApi.Domain.Constants;
using SlackApi.Domain.SlackDTOs;
using SlackApi.Repository.Cache;
using SlackApi.Repository.TableStorage;

namespace SlackApi.App.Services
{
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

        public async Task<SlackApiResponse> SelectBadgeAction(SlackInteractionPayload payload)
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
            
            await _slackApiClient.SlackPostRequest(SlackEndpoints.ViewsUpdateUrl, request);

            return new();
        }

        #region private methods

        private string GetBadgeViewIdCacheKey(string viewId) => $"NewBadgeView:{viewId}";

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
        public const string UserSelectionActionId = "select-user-action";
        public const string BadgeFeedbackActionId = "badge-feedback-action";

        // Blocks
        public const string BadgeImageBlockId = "badge-image-block";
        public const string BadgeFeedbackBlockId = "badge-feedback-block";
        public const string BadgeSelectBlockId = "badge-select-block";
    }

    public interface IBadgeViewModalService
    {
        Task<SlackResponse> OpenSendBadgeView(SlackInteractionPayload payload);
        Task<SlackApiResponse> SelectBadgeAction(SlackInteractionPayload payload);
    }
}
