namespace Legal_IA.Interfaces.Services;

public interface ICacheService
{
    /// <summary>
    ///     Gets a cached value by key.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <returns>The cached value if found, otherwise null.</returns>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    ///     Sets a value in the cache with an optional expiry.
    /// </summary>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiry">Optional expiry time for the cache entry.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;

    /// <summary>
    ///     Removes cache entries matching the specified pattern.
    /// </summary>
    /// <param name="pattern">The pattern to match cache keys.</param>
    Task RemoveByPatternAsync(string pattern);

    /// <summary>
    ///     Removes a cache entry by key.
    /// </summary>
    /// <param name="s">The cache key to remove.</param>
    Task RemoveAsync(string s);
}