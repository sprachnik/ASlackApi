using SlackApi.App.Builders;
using SlackApi.App.HttpClients;
using SlackApi.App.MemStore;
using SlackApi.Core.Exceptions;
using SlackApi.Core.Extensions;
using SlackApi.Domain.BadgeDTOs;
using SlackApi.Domain.Constants;
using SlackApi.Domain.DTOs;
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
        private readonly IUserNotificationService _userNotificationService;
        private readonly ISlackHomeTabService _slackHomeTabService;
        private readonly ISendMessageService _sendMessageService;
        private readonly string _placeHolderBadgeImageUrl = "https://i.imgur.com/Zam8zGt.jpg";

        public BadgeViewModalService(SlackApiClient slackApiClient,
            ITableStorageService tableStorageService,
            ITableStorageMemStore tableStorageMemStore,
            ICache cache,
            ISlackUserService slackUserService,
            IUserNotificationService userNotificationService,
            ISlackHomeTabService slackHomeTabService,
            ISendMessageService sendMessageService
            )
        {
            _slackApiClient = slackApiClient;
            _tableStorageService = tableStorageService;
            _tableStorageMemStore = tableStorageMemStore;
            _cache = cache;
            _slackUserService = slackUserService;
            _userNotificationService = userNotificationService;
            _slackHomeTabService = slackHomeTabService;
            _sendMessageService = sendMessageService;
        }

        public async Task<SlackResponse> OpenSendBadgeView(SlackInteractionPayload payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            var users = await _slackUserService.GetWorkspaceUsers(new PagedSlackRequestDTO
            {
                TeamId = payload?.Team?.Id,
                IsSavingOnFetch = true
            });

            var request = await GenerateSendABadgeView(new GenerateViewRequestDTO
            {
                CallbackId = payload?.CallbackId,
                TriggerId = payload?.TriggerId,
                BadgeImageUrl = _placeHolderBadgeImageUrl,
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

            var blockActionsToValidate = new List<BlockStateValueValidator>
            {
                new (BadgeFeedbackBlockId, v => !string.IsNullOrEmpty(v), "Please enter some feedback", BlockType.Input),
                new (BadgeSelectBlockId, v => !string.IsNullOrEmpty(v), ":exclamation: *Please select a badge*", BlockType.Section),
                new (UserSelectionBlockId, v => !string.IsNullOrEmpty(v), "Please select a user", BlockType.Input)
            };

            var blockActionValues = payload.GetBlockActionValues(blockActionsToValidate);

            // Only block inputs can be returned with errors
            if (blockActionValues != null && blockActionValues.Any(a => !a.IsValid && a.BlockType == BlockType.Input))
            {
                return new()
                {
                    ResponseAction = "errors",
                    Errors = blockActionValues
                        .Where(b => !b.IsValid && b.BlockType == BlockType.Input)
                        .Select(g => new KeyValuePair<string, string>(g.BlockId, g.ValidationMessage))
                        .ToDictionary(g => g.Key, g => g.Value)
                };
            }

            if (blockActionValues != null && blockActionValues.Any(a => !a.IsValid && a.BlockId == BadgeSelectBlockId))
            {
                var request = await GenerateSendABadgeView(new GenerateViewRequestDTO
                {
                    CallbackId = null,
                    TriggerId = null,
                    BadgeImageUrl = null,
                    BadgeText = null,
                    CurrentUserId = payload?.User?.Id,
                    BadgeSelectionActionValue = blockActionValues.FirstOrDefault(b => b.BlockId == BadgeSelectBlockId),
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

                return new()
                {
                    ResponseAction = "errors",
                    Errors = blockActionValues
                        .Where(b => !b.IsValid && b.BlockId == BadgeSelectBlockId)
                        .Select(g => new KeyValuePair<string, string>(g.BlockId, g.ValidationMessage))
                        .ToDictionary(g => g.Key, g => g.Value)
                };
            }

            await SaveBadge(blockActionValues, payload);

            return new();
        }

        private async Task SaveBadge(List<BlockActionValue>? blockActionValues, SlackInteractionPayload payload)
        {
            var badgeName = blockActionValues?.FirstOrDefault(b => b.BlockId == BadgeSelectBlockId)?.Value;
            var feedback = blockActionValues?.FirstOrDefault(b => b.BlockId == BadgeFeedbackBlockId)?.Value;
            var userId = blockActionValues?.FirstOrDefault(b => b.BlockId == UserSelectionBlockId)?.Value;

            if (string.IsNullOrWhiteSpace(badgeName)
                || string.IsNullOrWhiteSpace(feedback)
                || string.IsNullOrWhiteSpace(userId)
                || payload?.User == null)
                throw new ArgumentException("Check block action values and user in payload!");

            var selectedBadge = (await GetAllBadges())?.FirstOrDefault(b => b.Name == badgeName);

            if (selectedBadge == null)
                throw new ArgumentException($"{badgeName} badge not found!");

            var users = await _slackUserService.GetWorkspaceUsers(new PagedSlackRequestDTO
            {
                TeamId = payload?.User?.TeamId,
                IsSavingOnFetch = false
            });

            var fromUser = users?.FirstOrDefault(u => u.Id == payload?.User?.Id);

            var userBadge = new UserBadgeTableEntity(payload?.User?.TeamId,
                userId,
                payload?.User?.Id)
            {
                BadgeImageUrl = selectedBadge.ImageUrl,
                BadgeName = selectedBadge.Name,
                FromUserRealName = fromUser?.RealName is not null 
                    ? fromUser?.RealName 
                    : fromUser?.DisplayName is not null 
                    ? fromUser?.DisplayName : "User",
                Feedback = feedback
            };

            await _tableStorageService.UpsertAsync(userBadge, UserBadgeTableEntity.TableName);

            await SaveUserNotifications(userBadge, users);

            await _slackHomeTabService.Publish(payload?.User?.TeamId, payload?.User?.Id);
            await _slackHomeTabService.Publish(payload?.User?.TeamId, userId);

            await SendBadgeRecievedMessage(userBadge);
        }

        private async Task SendBadgeRecievedMessage(UserBadgeTableEntity userBadge)
        {
            var badgeSendMessage = new SlackMessageRequest
            {
                Channel = userBadge.UserId,
                Blocks = new BlockKitBuilder()
                   .AddAccessoryBlock(AccessoryType.Section, 
                   Guid.NewGuid().ToString(),
                   new Text
                   {
                       Type = TextType.Markdown,
                       BlockText = $"*{userBadge.FromUserRealName}* sent you a badge! :star:\n{userBadge.Feedback}"
                   },
                   new Accessory
                   {
                       Type = AccessoryType.Image,
                       ImageUrl = userBadge.BadgeImageUrl,
                       AltText = userBadge.BadgeName
                   }
                   ).GetBlocks()
            };

            await _sendMessageService.PostMessage(badgeSendMessage);
        }

        private async Task SaveUserNotifications(UserBadgeTableEntity? badge, List<SlackUserTableEntity>? users)
        {
            if (badge == null || users == null)
                throw new ArgumentException("Invalid arguments provided");

            var fromUser = users.FirstOrDefault(u => u.Id == badge.FromUserId);
            var toUser = users.FirstOrDefault(u => u.Id == badge.UserId);

            if (fromUser == null || toUser == null)
                throw new BusinessException("from or to user not found!");

            var badgeSentNotification = new UserNotificationTableEntity(badge.TeamId, fromUser?.Id,
                UserNotificationType.BadgeSent)
            {
                FromUserId = fromUser?.Id,
                FromUserRealName= fromUser?.RealName ?? fromUser?.DisplayName,
                ToUserId = toUser?.Id,
                ToUserRealName = toUser?.RealName ?? toUser?.DisplayName,
                Title = "Sent a badge",
                Body = badge.Feedback,
                ImageUrl = badge.BadgeImageUrl,
                Amount = 0
            };

            await _userNotificationService.InsertUserNotification(badgeSentNotification);

            var badgeReceivedNotification = new UserNotificationTableEntity(badge.TeamId, toUser?.Id,
                UserNotificationType.BadgeRecieved)
            {
                FromUserId = fromUser?.Id,
                FromUserRealName = fromUser?.RealName ?? fromUser?.DisplayName,
                ToUserId = toUser?.Id,
                ToUserRealName = toUser?.RealName ?? toUser?.DisplayName,
                Title = "Sent a badge",
                Body = badge.Feedback,
                ImageUrl = badge.BadgeImageUrl,
                Amount = 0
            };

            await _userNotificationService.InsertUserNotification(badgeReceivedNotification);
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

            viewBuilder = viewBuilder
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
                    });

            if (request.BadgeSelectionActionValue != null)
            {
                viewBuilder = viewBuilder.AddSingleContextBlock(new TextElement
                {
                    Type = ElementType.Markdown,
                    Text = request.BadgeSelectionActionValue.ValidationMessage
                });
            }

            return (SlackViewRequest)viewBuilder
                .AddDivider()
                .AddImageBlock(blockId: BadgeImageBlockId,
                    imageUrl: request.BadgeImageUrl ?? _placeHolderBadgeImageUrl,
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
            public BlockActionValue? BadgeSelectionActionValue { get; set; }
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
