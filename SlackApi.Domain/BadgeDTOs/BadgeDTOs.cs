using Azure;
using Azure.Data.Tables;

namespace SlackApi.Domain.BadgeDTOs
{
    public class BadgeTableEntity : TableEntityBase
    {
        public BadgeTableEntity()
        {

        }

        public BadgeTableEntity(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string? ImageUrl { get; set; }
        public string? Name { get; set; }
    }

    public class TableEntityBase : ITableEntity
    {
        public TableEntityBase()
        {

        }

        public TableEntityBase(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
        }

        public string? PartitionKey { get; set; }
        public string? RowKey { get; set; }
        public DateTime? DateDeleted { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }
    }
}
