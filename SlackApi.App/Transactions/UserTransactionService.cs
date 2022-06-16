using Microsoft.Extensions.Logging;
using SlackApi.Domain.Constants;
using SlackApi.Repository.Cache;
using SlackApi.Repository.TableStorage;

namespace SlackApi.App.Transactions
{
    public class UserTransactionService : IUserTransactionService
    {
        private readonly IUserBalanceService _userBalanceService;
        private readonly ITableStorageService _tableStorageService;
        private readonly ILogger<UserTransactionService> _logger;
        private readonly ICache _cache;

        public UserTransactionService(IUserBalanceService userBalanceService,
            ITableStorageService tableStorageService,
            ILogger<UserTransactionService> logger,
            ICache cache)
        {
            _userBalanceService = userBalanceService;
            _tableStorageService = tableStorageService;
            _logger = logger;
            _cache = cache;
        }

        public async Task<UserTransactionRecord> InsertTransaction(UserTransactionRecord transaction)
        {
            if (!transaction.IsValidTransaction())
                throw new InvalidOperationException($"{transaction} is not valid");

            transaction = await _userBalanceService.Withdraw(transaction);

            if (!transaction.IsFundsWithdrawn)
                return transaction;

            try
            {
                var newRecord = new UserTransactionTableEntity(transaction);
                await _tableStorageService.UpsertAsync(newRecord, TableStorageTable.UserTransactions);
                await GetTransactionBalance(transaction.ToUserId, isForce: true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
            }

            return transaction;
        }

        public async Task<UserTransactionBalance> GetTransactionBalance(string userId, bool isForce = false)
        {
            var cachKey = GetTransactionBalanceCacheKey(userId);

            if (isForce)
            {
                var records = await GetRecords(userId);
                await _cache.SetAsync(cachKey, records);
                return new(userId, records);
            }

            return new(userId, await _cache.GetAndOrSetAsync(cachKey, async () => await GetRecords(userId)));
        }

        private async Task<List<UserTransactionTableEntity>> GetRecords(string userId,
            string? fromEpochMillis = null)
        {
            var filter = $"PartitionKey eq '{userId}'";

            if (!string.IsNullOrEmpty(fromEpochMillis))
                filter += $" and RowKey ge '{fromEpochMillis}'";

            return await _tableStorageService.GetAllByQuery<UserTransactionTableEntity>(
                TableStorageTable.UserTransactions, filter);
        }

        private static string GetTransactionBalanceCacheKey(string userId) => $"GetTransactionBalance:{userId}";
    }
}
