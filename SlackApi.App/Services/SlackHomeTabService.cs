using SlackApi.App.Builders;
using SlackApi.App.HttpClients;
using SlackApi.Domain.Constants;
using SlackApi.Domain.DTOs;
using SlackApi.Domain.SlackDTOs;

namespace SlackApi.App.Services
{
    public class SlackHomeTabService : ISlackHomeTabService
    {
        private readonly SlackApiClient _slackApiClient;
        private readonly IUserNotificationService _userNotificationService;

        public SlackHomeTabService(SlackApiClient slackApiClient,
            IUserNotificationService userNotificationService)
        {
            _slackApiClient = slackApiClient;
            _userNotificationService = userNotificationService;
        }

        public async Task Publish(string? teamId, string? userId)
        {
            if (teamId == null || userId == null)
                throw new ArgumentNullException("teamId or userId is null");

            var userNotifications = await _userNotificationService
                .GetUserNotifications(teamId, userId, 100);

            var viewBuilder = new ViewBuilder(SlackResponsePayloadType.Home);

            if (userNotifications is not null && userNotifications.Any())
                foreach (var notification in userNotifications)
                    BuildNotification(notification, viewBuilder);

            var request = (SlackViewRequest)viewBuilder.ConstructRequest();

            await _slackApiClient.SlackPostRequest(SlackEndpoints.ViewPublishUrl, request);
        }

        private ViewBuilder BuildNotification(UserNotificationTableEntity? userNotification, ViewBuilder? viewBuilder)
        {
            if (userNotification == null
                || viewBuilder == null
                || userNotification.UserNotificationType == UserNotificationType.Default)
                throw new ArgumentNullException("Please check arguments aren't null");

            var body = userNotification.UserNotificationType switch
            {
                UserNotificationType.BadgeSent => $"*Sent a badge :star:* - {userNotification.ToUserRealName} \n{userNotification.Body}",
                UserNotificationType.BadgeRecieved => $"*Received a badge :star2:* \n{userNotification.Body}",
                _ => throw new NotImplementedException($"{userNotification.UserNotificationType} is not implemented!")
            };

            if (userNotification.UserNotificationType == UserNotificationType.BadgeRecieved
                || userNotification.UserNotificationType == UserNotificationType.BadgeSent)
            {
                viewBuilder.AddAccessoryBlock(BlockType.Section,
                    $"{userNotification.PartitionKey}_{userNotification.RowKey}",
                    new Text
                    {
                        Type = TextType.Markdown,
                        BlockText = body,
                    },
                    new Accessory
                    {
                        Type = AccessoryType.Image,
                        ImageUrl = userNotification.ImageUrl,
                        AltText = userNotification.Title
                    });

                if (userNotification.UserNotificationType == UserNotificationType.BadgeRecieved)
                {
                    viewBuilder.AddContextBlocks(new List<IElement>
                    {
                        new TextElement
                        {
                            Type = ElementType.PlainText,
                            Text = $"From: {userNotification.FromUserRealName}",
                        }
                    });
                }

                return viewBuilder.AddDivider();
            }

            return viewBuilder;
        }
    }

    public interface ISlackHomeTabService
    {
        Task Publish(string? teamId, string? userId);
    }
}
