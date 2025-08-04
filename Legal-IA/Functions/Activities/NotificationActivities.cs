using Legal_IA.DTOs;
using Legal_IA.Interfaces.Services;
using Legal_IA.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Legal_IA.Functions.Activities;

/// <summary>
///     Notification-related activity functions
/// </summary>
public class NotificationActivities(ILogger<NotificationActivities> logger, INotificationService notificationService)
{
    [Function("SendWelcomeNotificationActivity")]
    public async Task SendWelcomeNotificationActivity([ActivityTrigger] UserResponse user)
    {
        logger.LogInformation("Sending welcome notification to user {UserId} at {Email}", user.Id, user.Email);
        try
        {
            await notificationService.SendWelcomeNotificationAsync(user);
            logger.LogInformation("Welcome notification sent successfully to user {UserId}", user.Id);
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
        logger.LogInformation("Sending update notification to user {UserId}", user.Id);
        try
        {
            await notificationService.SendUserUpdateNotificationAsync(user);
            logger.LogInformation("Update notification sent successfully to user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error sending update notification to user {UserId}", user.Id);
            throw;
        }
    }
}