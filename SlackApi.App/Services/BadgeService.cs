using SlackApi.App.Builders;
using SlackApi.App.HttpClients;
using SlackApi.App.MemStore;
using SlackApi.Domain.BadgeDTOs;
using SlackApi.Domain.Constants;
using SlackApi.Domain.SlackDTOs;
using SlackApi.Repository.TableStorage;

namespace SlackApi.App.Services
{
    public class BadgeService : IBadgeService
    {
        private readonly SlackApiClient _slackApiClient;
        private readonly ITableStorageService _tableStorageService;
        private readonly ITableStorageMemStore _tableStorageMemStore;

        public BadgeService(SlackApiClient slackApiClient,
            ITableStorageService tableStorageService,
            ITableStorageMemStore tableStorageMemStore)
        {
            _slackApiClient = slackApiClient;
            _tableStorageService = tableStorageService;
            _tableStorageMemStore = tableStorageMemStore;
        }

        public async Task<SlackResponse> OpenSendBadgeView(SlackInteractionPayload payload)
        {
            if (payload == null)
                throw new ArgumentNullException(nameof(payload));

            var view = await GenerateSendABadgeView(payload?.CallbackId, payload?.TriggerId);
            await _slackApiClient.SlackPostRequest(SlackEndpoints.ViewsOpenUrl, view);

            return new SlackResponse();
        }

        #region private methods

        private async Task<SlackViewRequest> GenerateSendABadgeView(string? callbackId, string? triggerId)
        {
            if (string.IsNullOrWhiteSpace(callbackId))
                throw new ArgumentNullException(nameof(callbackId));

            if (string.IsNullOrWhiteSpace(triggerId))
                throw new ArgumentNullException(nameof(triggerId));

            return (SlackViewRequest)new ViewBuilder(SlackResponsePayloadType.Modal, callbackId, triggerId)
                .AddTitle("Send a badge")
                .AddBlock(BlockType.Section, 
                    blockId: "intro", 
                    new Text 
                    { 
                        Type = TextType.Markdown, 
                        BlockText = "*Select a funky badge for a colleague!*" 
                    })
                .AddUsersSelectBlock("Select a user", UserSelectionActionId)
                .AddBlock(BlockType.Section, 
                    blockId: BadgeSelectActionId,
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
                .AddSubmit(TextType.PlainText, "Submit")
                .ConstructRequest();
        }

        private async Task<List<BadgeTableEntity>> GetAllBadges()
        {
            var badges = await _tableStorageMemStore.Fetch<BadgeTableEntity>(TableStorageTable.Badges);
            return badges.Where(b => b.DateDeleted == null).ToList();
        }

        #endregion

        public readonly string BadgeSelectActionId = "badge-select";
        public readonly string UserSelectionActionId = "select-user-action";
    }

    public interface IBadgeService
    {
        Task<SlackResponse> OpenSendBadgeView(SlackInteractionPayload payload);
    }
}
