namespace SlackApi.App.Transactions
{
    public interface IUserTransactionService
    {
        Task<UserTransactionRecord> InsertTransaction(UserTransactionRecord transaction);
        Task<UserTransactionBalance> GetTransactionBalance(string userId, bool isForce = false);
    }
}