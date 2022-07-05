using SlackApi.Core.Extensions;
using System.Text.Json;

namespace SlackApi.App.Transactions
{
    public record UserTransactionRecord(string ToUserId, string FromUserId, double Amount,
        string Description, DateTime TransactionDate, Dictionary<string, string>? MetaData = null)
    {
        public string TransactionId => Guid.NewGuid().ToString();
        public string TransactionEpoch => TransactionDate.ToReverseTicksMillis().ToString("D16");
        public string MetaDataString => JsonSerializer.Serialize(MetaData);
        public bool IsFundsWithdrawn { get; set; } = false;
        public double BalanceAfterWithdrawl { get; set; } = 0;
        public string? Errors { get; set; } = null;
        public bool IsValidTransaction()
        {
            return !string.IsNullOrEmpty(ToUserId)
                && !string.IsNullOrEmpty(FromUserId)
                && !TransactionDate.Equals(DateTime.MinValue)
                && !TransactionDate.Equals(DateTime.MaxValue)
                && !Amount.Equals(double.MaxValue)
                && !Amount.Equals(double.MinValue)
                && !ToUserId.Equals(FromUserId);
        }
    }

    public record UserTransactionBalance(string UserId, List<UserTransactionTableEntity>? Records)
    {
        public double Balance => Records?.Where(r => r.UserId == UserId)?.Sum(r => r.Amount) ?? 0;
    }
}
