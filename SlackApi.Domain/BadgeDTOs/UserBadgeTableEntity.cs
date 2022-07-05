using SlackApi.Core.Extensions;
using SlackApi.Domain.TableStorage;

namespace SlackApi.Domain.BadgeDTOs
{
    public class UserBadgeTableEntity : TableEntityBase
    {
        public UserBadgeTableEntity()
        {

        }

        public UserBadgeTableEntity(string? teamId, string? userId, string? fromUserId) 
            : base(GeneratePartitionKey(teamId, userId), DateTime.UtcNow.ToReverseTicksMillis().ToString("D16"))
        {
            if (string.IsNullOrWhiteSpace(teamId) 
                || string.IsNullOrWhiteSpace(userId)
                || string.IsNullOrWhiteSpace(fromUserId))
                throw new ArgumentNullException("One of the arguments is null or empty!");

            DateCreated = DateTime.UtcNow;
            TeamId = teamId;
            UserId = userId;
            FromUserId = fromUserId;
        }

        public static string GeneratePartitionKey(string? teamId, string? userId) => string.IsNullOrWhiteSpace(teamId) 
            || string.IsNullOrWhiteSpace(userId) 
            ? throw new ArgumentNullException("One of the arguments is null or empty!") 
            : $"{teamId}_{userId}";

        public static string TableName => "TeamUserBadges";


        public DateTime DateCreated { get; set; }
        public string? TeamId { get; set; }
        public string? BadgeImageUrl { get; set; }
        public string? BadgeName { get; set; }
        public string? Name { get; set; }
        public string? FromUserId { get; set; }
        public string? FromUserRealName { get; set; }
        public string? UserId { get; set; }
        public string? Feedback { get; set; }
    }
}
