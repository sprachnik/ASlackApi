using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using SlackApi.Core.Extensions;
using SlackApi.Repository.Cache;
using SlackApi.Repository.TableStorage;

namespace SlackApi.App.MemStore
{
    public class TableStorageMemStore : ITableStorageMemStore
    {
        private readonly ITableStorageService _tableStorageService;
        private readonly ICache _cache;
        private readonly ILogger<TableStorageMemStore> _logger;
        private readonly string _storePrefix = "TableStorage";

        public TableStorageMemStore(ITableStorageService tableStorageService,
            ICache cache,
            ILogger<TableStorageMemStore> logger)
        {
            _tableStorageService = tableStorageService;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<T>> Init<T>(string storeName) where T : class, ITableEntity, new()
        {
            try
            {
                var cacheKey = GetStoreKey(storeName);
                await _cache.RemoveAsync(cacheKey);
                var entities = await _tableStorageService.GetAllAsync<T>(storeName);

                if (entities.IsNullOrEmpty())
                    return entities;

                await _cache.SetCollectionAsync(cacheKey, entities);
                var fetchedEntities = await _cache.GetCollectionAsync<T>(cacheKey);

                if (fetchedEntities == null || fetchedEntities.IsNullOrEmpty())
                    throw new Exception($"Unable to init table {storeName}!");

                return fetchedEntities;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex);
                throw;
            }
        }

        public async Task<List<T>> Fetch<T>(string storeName, bool isReInit = false) where T : class, ITableEntity, new()
        {
            var fetchedEntities = await _cache.GetCollectionAsync<T>(GetStoreKey(storeName));

            if (fetchedEntities != null && !fetchedEntities.IsNullOrEmpty())
                return fetchedEntities;

            if (isReInit)
                return await Init<T>(storeName);

            return new List<T>();
        }

        public string GetStoreKey(string storeName) => $"{_storePrefix}:Table:{storeName}";
    }

    public interface ITableStorageMemStore : IMemStore
    {

    }

    public interface IMemStore
    {
        Task<List<T>> Init<T>(string storeName) where T : class, ITableEntity, new();
        Task<List<T>> Fetch<T>(string storeName, bool isReInit = false) where T : class, ITableEntity, new();
        string GetStoreKey(string storeName);
    }
}
