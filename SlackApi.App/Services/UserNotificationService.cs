using SlackApi.Domain.DTOs;
using SlackApi.Repository.Cache;
using SlackApi.Repository.TableStorage;

namespace SlackApi.App.Services
{
    public class UserNotificationService : IUserNotificationService
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ICache _cache;
        public static string UserNotificationsTableName = "UserNotifications";
        private readonly TimeSpan _cacheTTL = TimeSpan.FromMinutes(10);

        public UserNotificationService(ITableStorageService tableStorageService,
            ICache cache)
        {
            _tableStorageService = tableStorageService;
            _cache = cache;
        }

        public async Task<List<UserNotificationTableEntity>?> GetUserNotifications(string? teamId, 
            string? userId, 
            int take = 100,
            bool isForce = false)
        {
            if (string.IsNullOrWhiteSpace(teamId)
              || string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException("One of the arguments is null or empty!");

            async Task<List<UserNotificationTableEntity>> GetUserNotificationsPaged()
            {
                var result = await _tableStorageService
                        .GetPagedCollectionAsync<UserNotificationTableEntity>(UserNotificationsTableName,
                        UserNotificationTableEntity.GeneratePartitionKey(teamId, userId),
                        new PagedTableStorageRequest
                        {
                            Take = take,
                            ContinuationToken = null
                        });

                return result?.Data ?? new List<UserNotificationTableEntity>();
            }

            return await _cache.GetAndOrSetAsync(GetUserNotificationsCacheKey(teamId, userId, take),
                GetUserNotificationsPaged, _cacheTTL, isForce);
        }

        public async Task InsertUserNotification(UserNotificationTableEntity userNotificationTableEntity)
        {
            await _tableStorageService.UpsertAsync(userNotificationTableEntity, UserNotificationsTableName);
            await GetUserNotifications(userNotificationTableEntity?.TeamId, 
                userNotificationTableEntity?.UserId, isForce: true);
        }

        private string GetUserNotificationsCacheKey(string? teamId, string? userId, int take)
            => $"UserNotifications:{teamId}:{userId}:{take}";
    }

    public interface IUserNotificationService
    {
        Task<List<UserNotificationTableEntity>?> GetUserNotifications(string? teamId,
            string? userId,
            int take = 100,
            bool isForce = false);

        Task InsertUserNotification(UserNotificationTableEntity userNotificationTableEntity);
    }
}
