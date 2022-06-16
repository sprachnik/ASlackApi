using SlackApi.Domain.TableStorage;

namespace SlackApi.Domain.BadgeDTOs
{
    public class BadgeTableEntity : TableEntityBase
    {
        public BadgeTableEntity()
        {

        }

        public BadgeTableEntity(string partitionKey, string rowKey) : base(partitionKey, rowKey)
        {

        }

        public string? ImageUrl { get; set; }
        public string? Name { get; set; }
    }
}
