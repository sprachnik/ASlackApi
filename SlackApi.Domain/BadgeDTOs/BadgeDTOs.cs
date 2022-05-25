using Azure;
using Azure.Data.Tables;

namespace SlackApi.Domain.BadgeDTOs
{
    public class BadgeTableEntity : ITableEntity
    {
        public BadgeTableEntity()
        {

        }

        public BadgeTableEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string ImageUrl { get; set; }
        public string Name { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTime? DateDeleted { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
