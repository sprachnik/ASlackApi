using Microsoft.Extensions.Logging;
using SlackApi.Core.Exceptions;
using SlackApi.Core.Extensions;
using SlackApi.Core.Settings;
using SlackApi.Domain.Constants;
using SlackApi.Repository.Cache;
using SlackApi.Repository.TableStorage;

namespace SlackApi.App.Transactions
{
    public class UserBalanceService : IUserBalanceService
    {
        private readonly ICache _cache;
        private readonly ITableStorageService _tableStorageService;
        private readonly ApplicationSettings _applicationSettings;
        private readonly ILogger<UserBalanceService> _logger;

        public UserBalanceService(ICache cache,
            ITableStorageService tableStorageService,
            ApplicationSettings applicationSettings,
            ILogger<UserBalanceService> logger)
        {
            _cache = cache;
            _tableStorageService = tableStorageService;
            _applicationSettings = applicationSettings;
            _logger = logger;
        }

        public async Task<UserBalanceTransactionTableEntity> Deposit(string userId, double amount, string? description = null)
        {
            if (string.IsNullOrEmpty(userId))
                throw new ArgumentNullException(nameof(userId));

            if (amount > _applicationSettings.MaxTransactionSize)
                throw new BusinessException("Amount exceeds max transaction size!");

            var transactionDate = DateTime.UtcNow;

            var newRecord = new UserBalanceTransactionTableEntity(
                new UserBalanceTransactionRecord(userId, userId, amount,
                    description ?? "Automated Deposit", transactionDate, Guid.NewGuid().ToString(),
                    transactionDate.ToReverseTicksMillis().ToString("D16")));

            await _tableStorageService.UpsertAsync(newRecord, TableStorageTable.UserBalanceTransactions);
            await _cache.RemoveAsync(GetBalanceCacheKey(userId, transactionDate.GetStartOfWeekReverseTickEpochMillis()));

            return newRecord;
        }

        public async Task<UserTransactionRecord> Withdraw(UserTransactionRecord userTransactionRecord)
        {
            if (!userTransactionRecord.IsValidTransaction())
                throw new BusinessException($"Invalid Withdraw request!");

            var usersBalance = await GetBalance(userTransactionRecord.FromUserId);

            if (usersBalance.Balance <= 0 || usersBalance.Balance < userTransactionRecord.Amount)
            {
                userTransactionRecord.Errors = $"{userTransactionRecord.Amount} is greater than your " +
                    $"balance of {usersBalance.Balance}. \n" +
                    $"Your credits reset every monday.";

                return userTransactionRecord;
            }

            try
            {
                var now = DateTime.UtcNow;

                var newRecord = new UserBalanceTransactionTableEntity(
                    new UserBalanceTransactionRecord(userTransactionRecord.FromUserId,
                        userTransactionRecord.ToUserId,
                        -userTransactionRecord.Amount, userTransactionRecord.Description ?? "Withdrawl",
                        now, userTransactionRecord.TransactionId, userTransactionRecord.TransactionEpoch, 
                        userTransactionRecord.MetaData));

                await _tableStorageService.UpsertAsync(newRecord, TableStorageTable.UserBalanceTransactions);
                usersBalance = await GetBalance(userTransactionRecord.FromUserId, isForce: true);

                userTransactionRecord.IsFundsWithdrawn = true;
                userTransactionRecord.BalanceAfterWithdrawl = usersBalance.Balance;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                userTransactionRecord.Errors = $"Oops! There was an error processing your transaction. Try again later...";
            }

            return userTransactionRecord;
        }

        public async Task<UserCurrentBalanceRecord> GetBalance(string userId, bool isForce = false)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException(nameof(userId));

            var now = DateTime.UtcNow;
            var startOfWeekDate = now.GetStartOfWeekDateTime();
            var startOfWeek = now.GetStartOfWeekReverseTickEpochMillis();

            async Task<List<UserBalanceTransactionTableEntity>> GetRecords()
            {
                var filter = $"PartitionKey eq '{userId}' and RowKey le '{startOfWeek}'";

                var records = await _tableStorageService.GetAllByQuery<UserBalanceTransactionTableEntity>(
                    TableStorageTable.UserBalanceTransactions, filter);

                if (records.IsNullOrEmpty())
                {
                    records = new List<UserBalanceTransactionTableEntity> 
                    { 
                        await Deposit(userId, _applicationSettings.DefaultWeeklyBalance, "Weekly Automated Deposit") 
                    };
                }

                return records;
            }

            var cacheKey = GetBalanceCacheKey(userId, startOfWeek);
            var ttl = startOfWeekDate.AddDays(7) - now;

            if (isForce)
            {
                var records = await GetRecords();
                await _cache.SetAsync(cacheKey, records, ttl);
                return new(userId, records);
            }

            return new(userId, await _cache.GetAndOrSetAsync(cacheKey, GetRecords, ttl));
        }

        private static string GetBalanceCacheKey(string userId, string startOfWeekTicks) => $"GetBalance:{userId}:{startOfWeekTicks}";
    }
}
