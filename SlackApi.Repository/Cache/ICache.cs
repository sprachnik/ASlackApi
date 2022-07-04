namespace SlackApi.Repository.Cache
{
    public interface ICache
    {
        Task SetAsync<T>(string key, T value, TimeSpan? ttl = null);
        Task SetCollectionAsync<T>(string key, List<T> values, TimeSpan? ttl = null);
        Task<T?> GetAsync<T>(string key);
        Task<List<T>?> GetCollectionAsync<T>(string key);
        Task RemoveAsync(string key);
        Task<T?> GetAndOrSetAsync<T>(string key,
            Func<Task<T>> func, TimeSpan? ttl = null, bool isForce = false);
    }
}
