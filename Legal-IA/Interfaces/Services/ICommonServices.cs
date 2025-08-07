using Legal_IA.DTOs;

namespace Legal_IA.Interfaces.Services;

/// <summary>
///     Service interface for notification operations
/// </summary>
public interface INotificationService
{
    Task SendWelcomeNotificationAsync(UserResponse user);
    Task SendUserUpdateNotificationAsync(UserResponse user);
}