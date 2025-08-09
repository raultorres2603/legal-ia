using Legal_IA.DTOs;
using Legal_IA.Interfaces.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Activities;

/// <summary>
///     Notification-related activity functions
/// </summary>
public class NotificationActivities(ILogger<NotificationActivities> logger, INotificationService notificationService)
{
    [Function("SendWelcomeNotificationActivity")]
    public async Task SendWelcomeNotificationActivity([ActivityTrigger] UserResponse user)
    {
        logger.LogDebug($"[SendWelcomeNotificationActivity] Activity started for user {user.Id}");
        try
        {
            await notificationService.SendWelcomeNotificationAsync(user);
            logger.LogInformation("Welcome notification sent successfully to user {UserId}", user.Id);
            logger.LogDebug($"[SendWelcomeNotificationActivity] Activity completed for user {user.Id}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending welcome notification to user {UserId}", user.Id);
            throw;
        }
    }

    [Function("SendUserUpdateNotificationActivity")]
    public async Task SendUserUpdateNotificationActivity([ActivityTrigger] UserResponse user)
    {
        logger.LogInformation($"[SendUserUpdateNotificationActivity] Activity started for user {user.Id}");
        try
        {
            await notificationService.SendUserUpdateNotificationAsync(user);
            logger.LogInformation("Update notification sent successfully to user {UserId}", user.Id);
            logger.LogInformation($"[SendUserUpdateNotificationActivity] Activity completed for user {user.Id}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending update notification to user {UserId}", user.Id);
            throw;
        }
    }
}