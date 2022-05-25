using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace SlackApi.Repository.Cache
{
    public class MemCache : ICache
    {
        private readonly IDistributedCache _distributedCache;

        public MemCache(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task SetAsync<T>(string key, T value)
            => await _distributedCache.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(value));

        public async Task SetCollectionAsync<T>(string key, List<T> values)
            => await _distributedCache.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(values));

        public async Task<T?> GetAsync<T>(string key)
        {
            var response = await _distributedCache.GetAsync(key);

            if (response == null)
                return default;

            return JsonSerializer.Deserialize<T>(response);
        }

        public async Task<List<T>?> GetCollectionAsync<T>(string key)
        {
            var response = await _distributedCache.GetAsync(key);

            if (response == null)
                return default;

            return JsonSerializer.Deserialize<List<T>>(response);
        }

        public async Task RemoveAsync(string key)
            => await _distributedCache.RemoveAsync(key);

        public async Task<T?> GetAndOrSetAsync<T>(string key, Func<Task<T>> func)
        {
            var response = await GetAsync<T>(key);

            if (response is not null)
                return response;

            var obj = await func();

            if (obj is not null)
                await SetAsync(key, obj);

            return obj;
        }
    }
}
