using SlackApi.Core.Extensions;
using System.Text.Json;

namespace SlackApi.App.Transactions
{
    public record UserBalanceTransactionRecord(string UserId, string ToUserId, double Amount,
        string Description, DateTime TransactionDate, string TransactionId, string TransactionEpoch,
        Dictionary<string, string>? MetaData = null)
    {
        public string MetaDataString => JsonSerializer.Serialize(MetaData);

        public bool IsValidTransaction()
        {
            return !string.IsNullOrEmpty(ToUserId)
                && !string.IsNullOrEmpty(UserId)
                && !TransactionDate.Equals(DateTime.MinValue)
                && !TransactionDate.Equals(DateTime.MaxValue)
                && !Amount.Equals(decimal.MaxValue)
                && !Amount.Equals(decimal.MinValue);
        }
    }

    public record UserCurrentBalanceRecord(string UserId, List<UserBalanceTransactionTableEntity>? UserBalances)
    {
        public double Balance => UserBalances?.Where(r => r.UserId == UserId)?.Sum(r => r.Amount) ?? 0;
    }
}
