namespace QuakeNote.Shared.Cache;

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan ttl) where T : class;
    Task RemoveAsync(string key);
    Task RemoveByPrefixAsync(string prefix);
    Task<bool> ExistsAsync(string key);

    /// <summary>Cache-aside: взять из кэша, а при промахе вычислить, сохранить и вернуть.</summary>
    Task<T> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<Task<T>> factory) where T : class;
}