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
            request.UserId = userId;

            await _slackApiClient.SlackPostRequest($"{SlackEndpoints.ViewPublishUrl}", request);
        }

        private ViewBuilder BuildNotification(UserNotificationTableEntity? userNotification, ViewBuilder? viewBuilder)
        {
            if (userNotification == null
                || viewBuilder == null
                || userNotification.UserNotificationType == UserNotificationType.Default)
                throw new ArgumentNullException("Please check arguments aren't null");

            viewBuilder = userNotification.UserNotificationType switch
            {
                UserNotificationType.BadgeSent => GenerateBadgeSentNotification(viewBuilder, userNotification),
                UserNotificationType.BadgeRecieved => GenerateBadgeRecievedNotification(viewBuilder, userNotification),
                _ => throw new NotImplementedException($"{userNotification.UserNotificationType} is not implemented!")
            };

            return viewBuilder;
        }

        private ViewBuilder GenerateBadgeRecievedNotification(ViewBuilder viewBuilder,
            UserNotificationTableEntity userNotification)
        {
            return viewBuilder.AddAccessoryBlock(BlockType.Section,
                    $"{userNotification.PartitionKey}_{userNotification.RowKey}",
                    new Text
                    {
                        Type = TextType.Markdown,
                        BlockText = $"*Received a badge :star2:* \n{userNotification.Body}",
                    },
                    new Accessory
                    {
                        Type = AccessoryType.Image,
                        ImageUrl = userNotification.ImageUrl,
                        AltText = userNotification.Title
                    })
                .AddSingleContextBlock(new TextElement
                    {
                        Type = TextType.Markdown,
                        Text = $"Sent from: *{userNotification.FromUserRealName}* - {userNotification.DateCreated:yyyy-MM-dd HH:mm}"
                    })
                .AddDivider();
        }

        private ViewBuilder GenerateBadgeSentNotification(ViewBuilder viewBuilder, 
            UserNotificationTableEntity userNotification)
        {
            return viewBuilder.AddAccessoryBlock(BlockType.Section,
                    $"{userNotification.PartitionKey}_{userNotification.RowKey}",
                    new Text
                    {
                        Type = TextType.Markdown,
                        BlockText = $"*You sent a badge :star:* \n{userNotification.Body}",
                    },
                    new Accessory
                    {
                        Type = AccessoryType.Image,
                        ImageUrl = userNotification.ImageUrl,
                        AltText = userNotification.Title
                    })
                .AddSingleContextBlock(new TextElement
                    {
                        Type = TextType.Markdown,
                        Text = $"Sent to: *{userNotification.ToUserRealName}* - {userNotification.DateCreated:yyyy-MM-dd HH:mm}"
                    }).AddDivider();
        }
    }

    public interface ISlackHomeTabService
    {
        Task Publish(string? teamId, string? userId);
    }
}
