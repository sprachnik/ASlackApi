using SlackApi.App.Builders;
using SlackApi.App.HttpClients;
using SlackApi.App.MemStore;
using SlackApi.Core.Exceptions;
using SlackApi.Core.Extensions;
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
        private readonly ISlackUserService _slackUserService;
        private readonly string _placeHolderImageShrugUrl = "https://i.imgur.com/Zam8zGt.jpg";

        public BadgeViewModalService(SlackApiClient slackApiClient,
            ITableStorageService tableStorageService,
            ITableStorageMemStore tableStorageMemStore,
            ICache cache,
            ISlackUserService slackUserService)
        {
            _slackApiClient = slackApiClient;
            _tableStorageService = tableStorageService;
            _tableStorageMemStore = tableStorageMemStore;
            _cache = cache;
            _slackUserService = slackUserService;
        }

        public async Task<SlackResponse> OpenSendBadgeView(SlackInteractionPayload payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            var users = await _slackUserService.GetWorkspaceUsers(new PagedSlackRequestDTO
            {
                TeamId = payload?.Team?.Id,
                IsSavingOnFetch = false
            });

            var request = await GenerateSendABadgeView(new GenerateViewRequestDTO
            {
                CallbackId = payload?.CallbackId,
                TriggerId = payload?.TriggerId,
                BadgeImageUrl = _placeHolderImageShrugUrl,
                CurrentUserId = payload?.User?.Id,
                Users = users
            });

            await _slackApiClient.SlackPostRequest(SlackEndpoints.ViewsOpenUrl, request);

            return new();
        }

        public async Task<SlackResponse> SubmitBadgeView(SlackInteractionPayload payload)
        {
            if (payload?.View?.RootViewId == null)
                throw new ArgumentNullException(nameof(payload));

            var blockActionsToValidate = new Dictionary<string, string>
            {
                { BadgeFeedbackBlockId, "Please enter some feedback" },
                { BadgeSelectBlockId, "Please select a badge" },
                { UserSelectionBlockId, "Please select a user" }
            };

            var (blockErrors, blockActionValues) = payload.ValidateBlockActions(blockActionsToValidate);

            if (blockErrors != null && blockErrors.Any())
            {
                return new()
                {
                    ResponseAction = "errors",
                    Errors = blockErrors
                };
            }


            return new();
        }

        public async Task<SlackResponse> SelectUserAction(SlackInteractionPayload payload)
        {
            if (payload == null || payload?.View?.RootViewId == null)
                throw new ArgumentNullException(nameof(payload));

            var selectedUserAction = payload.Actions?.FirstOrDefault(s => s.SelectedUser != null);
            var selectAction = payload.Actions?.FirstOrDefault(s => s.SelectedOption != null);

            var slackUserId = selectedUserAction?.SelectedUser ?? selectAction?.SelectedOption?.Value;

            if (slackUserId == null)
                throw new ArgumentNullException(nameof(slackUserId));

            if (slackUserId.Equals(payload?.User?.Id))
                return new();

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

            var request = await GenerateSendABadgeView(new GenerateViewRequestDTO
            {
                CallbackId = null,
                TriggerId = null,
                BadgeImageUrl = selectedBadge.ImageUrl,
                BadgeText = selectedBadge.Name,
                CurrentUserId = payload?.User?.Id,
                Users = await _slackUserService.GetWorkspaceUsers(new PagedSlackRequestDTO
                {
                    TeamId = payload?.Team?.Id,
                    IsSavingOnFetch = false
                })
            });
            
            if (request?.View == null)
                throw new ArgumentNullException(nameof(request));

            request.ViewId = payload?.View?.RootViewId;
            request.Hash = payload?.View?.Hash;
            
            await _slackApiClient.SlackPostRequest(SlackEndpoints.ViewsUpdateUrl, request);

            return new();
        }

        #region private methods

        private async Task<SlackViewRequest> GenerateSendABadgeView(GenerateViewRequestDTO request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var viewBuilder = new ViewBuilder(SlackResponsePayloadType.Modal, request.CallbackId, request.TriggerId)
                .AddTitle("Send a badge")
                .AddAccessoryBlock(BlockType.Section,
                    blockId: "intro",
                    new Text
                    {
                        Type = TextType.Markdown,
                        BlockText = "Let your coworker know how much you appreciate them by sending them a badge with some positive feedback :star2:"
                    });

            var nonBotUsers = request.Users
                ?.Where(u => u.IsBot == false 
                    && u.DisplayName?.ToLower()?.Contains("slack") == false
                    && (string.IsNullOrWhiteSpace(u.DisplayName) == false || string.IsNullOrWhiteSpace(u.RealName) == false)
                    && u.Id != request.CurrentUserId)?.ToList();

            if (nonBotUsers == null || nonBotUsers.IsNullOrEmpty())
                viewBuilder = viewBuilder.AddUsersSelectBlock("Pick your coworker :point_right:", UserSelectionActionId, UserSelectionBlockId);
            else
                viewBuilder = viewBuilder.AddInputBlock(UserSelectionBlockId,
                    new Element
                    {
                        Type = AccessoryType.StaticSelect,
                        Placeholder = new Placeholder
                        {
                            Type = TextType.PlainText,
                            Text = "Coworkers...",
                            Emoji = true
                        },
                        Options = nonBotUsers.Select(user => new Option
                        {
                            Text = new Text
                            {
                                Type = TextType.PlainText,
                                BlockText = user.RealName ?? user.DisplayName
                            },
                            Value = user.Id
                        }).ToList(),
                        ActionId = UserSelectionActionId
                    },
                    new Label { Type = TextType.PlainText, Text = "Pick your coworker :point_down:" });

            return (SlackViewRequest)viewBuilder
                .AddDivider()
                .AddAccessoryBlock(BlockType.Section,
                    blockId: BadgeSelectBlockId,
                    new Text
                    {
                        Type = TextType.PlainText,
                        BlockText = "Select an badge :trophy:",
                        Emoji = true
                    },
                    new Accessory
                    {
                        Type = AccessoryType.StaticSelect,
                        ActionId = BadgeSelectActionId,
                        Placeholder = new Placeholder
                        {
                            Type = TextType.PlainText,
                            Text = "Badges..."
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
                .AddDivider()
                .AddImageBlock(blockId: BadgeImageBlockId,
                    imageUrl: request.BadgeImageUrl ?? _placeHolderImageShrugUrl,
                    altText: request.BadgeText ?? "Select a badge...",
                    new Title
                    {
                        Text = request.BadgeText ?? "Select a badge...",
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
        public const string UserSelectionBlockId = "give-badge-select-user-block";
        public const string BadgeImageBlockId = "give-badge-image-block";
        public const string BadgeFeedbackBlockId = "give-badge-feedback-block";
        public const string BadgeSelectBlockId = "give-badge-select-block";

        internal class GenerateViewRequestDTO
        {
            public string? CallbackId { get; set; }
            public string? TriggerId { get; set; }
            public string? BadgeImageUrl { get; set; }
            public string? BadgeText { get; set; }
            public List<SlackUserTableEntity>? Users { get; set; }
            public string? CurrentUserId { get; set; }
        }
    }

    public interface IBadgeViewModalService
    {
        Task<SlackResponse> OpenSendBadgeView(SlackInteractionPayload payload);
        Task<SlackResponse> SubmitBadgeView(SlackInteractionPayload payload);
        Task<SlackResponse> SelectBadgeAction(SlackInteractionPayload payload);
        Task<SlackResponse> SelectUserAction(SlackInteractionPayload payload);
    }
}
