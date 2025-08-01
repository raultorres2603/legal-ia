using Legal_IA.DTOs;

namespace Legal_IA.Interfaces.Services;

/// <summary>
///     Service interface for caching operations
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
}

/// <summary>
///     Service interface for notification operations
/// </summary>
public interface INotificationService
{
    Task SendWelcomeNotificationAsync(UserResponse user);
    Task SendUserUpdateNotificationAsync(UserResponse user);
}