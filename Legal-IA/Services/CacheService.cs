using System.Text.Json;
using Legal_IA.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Legal_IA.Services;

/// <summary>
///     Cache service implementation using Redis
/// </summary>
public class CacheService(IDistributedCache cache, IConnectionMultiplexer redis, ILogger<CacheService> logger)
    : ICacheService
{
    private readonly TimeSpan _defaultExpiry = TimeSpan.FromMinutes(30);

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var cachedValue = await cache.GetStringAsync(key);
        if (string.IsNullOrEmpty(cachedValue))
        {
            logger.LogInformation($"[CacheService] Cache miss for key: {key}");
            return null;
        }

        logger.LogInformation($"[CacheService] Cache hit for key: {key}");
        return JsonSerializer.Deserialize<T>(cachedValue);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class
    {
        var serializedValue = JsonSerializer.Serialize(value);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiry ?? _defaultExpiry
        };
        await cache.SetStringAsync(key, serializedValue, options);
        logger.LogInformation(
            $"[CacheService] Cache set for key: {key} (expiry: {(expiry ?? _defaultExpiry).TotalSeconds} seconds)");
    }

    public async Task RemoveAsync(string key)
    {
        logger.LogInformation($"[CacheService] Removing cache key: {key}");
        await cache.RemoveAsync(key);
    }

    public async Task RemoveByPatternAsync(string pattern)
    {
        logger.LogInformation($"[CacheService] Removing cache keys by pattern: {pattern}*");
        var endpoints = redis.GetEndPoints();
        foreach (var endpoint in endpoints)
        {
            var server = redis.GetServer(endpoint);
            var keys = server.Keys(pattern: pattern + "*");
            foreach (var key in keys)
            {
                logger.LogInformation($"[CacheService] Removing cache key (pattern match): {key}");
                await RemoveAsync(key!);
            }
        }
    }
}