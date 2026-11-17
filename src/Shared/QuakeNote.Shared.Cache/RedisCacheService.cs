using System.Text.Json;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace QuakeNote.Shared.Cache;

public sealed class RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    : ICacheService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private IDatabase Db => redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        try
        {
            var value = await Db.StringGetAsync(key);
            return value.IsNullOrEmpty ? null : JsonSerializer.Deserialize<T>(value.ToString(), JsonOptions);
        }
        catch (Exception ex) when (ex is RedisException or JsonException)
        {
            logger.LogWarning(ex, "Cache read failed for key {Key}", key);
            return null;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan ttl) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            await Db.StringSetAsync(key, json, ttl);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Cache write failed for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key)
    {
        try
        {
            await Db.KeyDeleteAsync(key);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Cache delete failed for key {Key}", key);
        }
    }

    public async Task RemoveByPrefixAsync(string prefix)
    {
        try
        {
            foreach (var endpoint in redis.GetEndPoints())
            {
                var server = redis.GetServer(endpoint);
                if (!server.IsConnected || server.IsReplica) continue;

                await foreach (var key in server.KeysAsync(pattern: prefix + "*"))
                {
                    await Db.KeyDeleteAsync(key);
                }
            }
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Cache prefix delete failed for prefix {Prefix}", prefix);
        }
    }

    public async Task<bool> ExistsAsync(string key)
    {
        try
        {
            return await Db.KeyExistsAsync(key);
        }
        catch (RedisException ex)
        {
            logger.LogWarning(ex, "Cache exists check failed for key {Key}", key);
            return false;
        }
    }

    public async Task<T> GetOrCreateAsync<T>(string key, TimeSpan ttl, Func<Task<T>> factory) where T : class
    {
        var cached = await GetAsync<T>(key);
        if (cached is not null) return cached;

        var value = await factory();
        await SetAsync(key, value, ttl);
        return value;
    }
}