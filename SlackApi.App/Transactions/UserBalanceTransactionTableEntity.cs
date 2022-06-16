using SlackApi.Domain.TableStorage;
using System.Runtime.Serialization;
using System.Text.Json;

namespace SlackApi.App.Transactions
{
    public class UserBalanceTransactionTableEntity : TableEntityBase
    {
        public UserBalanceTransactionTableEntity()
        {

        }

        public UserBalanceTransactionTableEntity(UserBalanceTransactionRecord transaction) : base(transaction.UserId, transaction.TransactionEpoch)
        {
            UserId = transaction.UserId;
            ToUserId = transaction.ToUserId;
            Description = transaction.Description;
            MetaDataString = transaction.MetaDataString;
            Amount = transaction.Amount;
            TransactionDate = transaction.TransactionDate;
            TransactionId = transaction.TransactionId;
        }

        public string TransactionId { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string ToUserId { get; set; } = string.Empty;
        public double Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string MetaDataString { get; set; } = string.Empty;

        [IgnoreDataMember]
        public Dictionary<string, string>? MetaData => !string.IsNullOrWhiteSpace(MetaDataString)
            ? JsonSerializer.Deserialize<Dictionary<string, string>>(MetaDataString) : new Dictionary<string, string>();
    }
}
