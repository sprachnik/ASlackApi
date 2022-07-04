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

        public async Task SetAsync<T>(string key, T value, TimeSpan? ttl = null)
            => await _distributedCache.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(value), 
                new DistributedCacheEntryOptions().SetSlidingExpiration(ttl ?? TimeSpan.FromDays(365)));

        public async Task SetCollectionAsync<T>(string key, List<T> values, TimeSpan? ttl = null)
            => await _distributedCache.SetAsync(key, JsonSerializer.SerializeToUtf8Bytes(values),
                new DistributedCacheEntryOptions().SetSlidingExpiration(ttl ?? TimeSpan.FromDays(365)));

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

        public async Task<T?> GetAndOrSetAsync<T>(string key, 
            Func<Task<T>> func, TimeSpan? ttl = null, bool isForce = false)
        {
            var response = isForce ? default : await GetAsync<T>(key);

            if (response is not null)
                return response;

            var obj = await func();

            if (obj is not null)
                await SetAsync(key, obj, ttl);

            return obj;
        }
    }
}
