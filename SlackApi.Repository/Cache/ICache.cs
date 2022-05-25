namespace SlackApi.Repository.Cache
{
    public interface ICache
    {
        Task SetAsync<T>(string key, T value);
        Task SetCollectionAsync<T>(string key, List<T> values);
        Task<T?> GetAsync<T>(string key);
        Task<List<T>?> GetCollectionAsync<T>(string key);
        Task RemoveAsync(string key);
        Task<T?> GetAndOrSetAsync<T>(string key, Func<Task<T>> func);
    }
}
