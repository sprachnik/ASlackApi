using Azure;
using Azure.Data.Tables;

namespace SlackApi.Domain.TableStorage
{
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
