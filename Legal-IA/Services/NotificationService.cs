using Legal_IA.DTOs;
using Legal_IA.Functions.Activities;
using Legal_IA.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Services;

/// <summary>
///     Notification service implementation
/// </summary>
public class NotificationService(
    ILogger<RegisterUserActivity> logger,
    IEmailService emailService,
    IConfiguration configuration)
    : INotificationService
{
    public async Task SendWelcomeNotificationAsync(UserResponse user)
    {
        logger.LogInformation("Sending welcome notification to user {UserId} at {Email}", user.Id, user.Email);

        // TODO: Implement actual notification logic (email, SMS, push notifications)
        // For now, we'll just log the action
        await Task.Delay(100); // Simulate async operation

        logger.LogInformation("Welcome notification sent successfully to {Email}", user.Email);
    }

    public async Task SendUserUpdateNotificationAsync(UserResponse user)
    {
        logger.LogInformation("Sending update notification to user {UserId}", user.Id);

        // TODO: Implement actual notification logic
        await Task.Delay(100); // Simulate async operation

        logger.LogInformation("Update notification sent successfully to user {UserId}", user.Id);
    }

    public async Task SendEmailVerificationAsync(string email, string firstName, string verificationToken)
    {
        logger.LogInformation("Sending email verification to {Email}", email);
        var hostname = configuration["Hostname"] ?? "http://localhost:7180";
        var verificationLink = $"{hostname}/api/user/verify?token={verificationToken}";
        await emailService.SendVerificationEmailAsync(email, firstName, verificationLink);
        logger.LogInformation("Email verification sent to {Email}", email);
    }
}