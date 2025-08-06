using Legal_IA.Interfaces.Services;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Legal_IA.Services;

/// <summary>
///     Cache service implementation using Redis
/// </summary>
public class CacheService(IDistributedCache cache) : ICacheService
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
        // For Redis, you would need to implement pattern-based removal
        // This is a simplified implementation
        // In a real scenario, you might use Redis-specific commands
        throw new NotImplementedException("Pattern-based cache removal requires Redis-specific implementation");
    }
}