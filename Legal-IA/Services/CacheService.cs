using Legal_IA.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Text.Json;

namespace Legal_IA.Services;

/// <summary>
///     Cache service implementation using Redis
/// </summary>
public class CacheService(IDistributedCache cache, IConnectionMultiplexer redis) : ICacheService
{
    private readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(30);

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var cachedValue = await cache.GetStringAsync(key);
        return string.IsNullOrEmpty(cachedValue)
            ? null
            : JsonSerializer.Deserialize<T>(cachedValue);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var serializedValue = JsonSerializer.Serialize(value);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? _defaultExpiry
        };
        await cache.SetStringAsync(key, serializedValue, options);
    }

    public async Task RemoveAsync(string key)
    {
        await cache.RemoveAsync(key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        var endpoints = redis.GetEndPoints();
        var server = redis.GetServer(endpoints.First());
        var keys = server.Keys(pattern: pattern + "*");
        var tasks = keys.Select(key => cache.RemoveAsync(key));
        await Task.WhenAll(tasks);
    }
}