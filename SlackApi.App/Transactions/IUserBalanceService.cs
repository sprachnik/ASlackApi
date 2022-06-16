namespace SlackApi.App.Transactions
{
    public interface IUserBalanceService
    {
        Task<UserBalanceTransactionTableEntity> Deposit(string userId, double amount, string? description = null);
        Task<UserTransactionRecord> Withdraw(UserTransactionRecord userTransactionRecord);
        Task<UserCurrentBalanceRecord> GetBalance(string userId, bool isForce = false);
    }
}
