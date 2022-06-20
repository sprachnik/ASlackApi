using SlackApi.Domain.TableStorage;

namespace SlackApi.Domain.BadgeDTOs
{
    public class UserBadgeTableEntity : TableEntityBase
    {
        public UserBadgeTableEntity()
        {

        }

        public UserBadgeTableEntity(string partitionKey, string rowKey) : base(partitionKey, rowKey)
        {

        }

        public string? ImageUrl { get; set; }
        public string? Name { get; set; }
        public string? FromUserId { get; set; }
        public string? FromUserDisplayName { get; set; }
        public string? UserId { get; set; }
        public string? Feedback { get; set; }
    }
}
