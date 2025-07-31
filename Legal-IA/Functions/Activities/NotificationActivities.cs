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
        await notificationService.SendWelcomeNotificationAsync(user);
    }

    [Function("SendUserUpdateNotificationActivity")]
    public async Task SendUserUpdateNotificationActivity([ActivityTrigger] UserResponse user)
    {
        logger.LogInformation("Sending update notification to user {UserId}", user.Id);
        await notificationService.SendUserUpdateNotificationAsync(user);
    }

    [Function("SendDocumentGenerationNotificationActivity")]
    public async Task SendDocumentGenerationNotificationActivity([ActivityTrigger] DocumentResponse document)
    {
        logger.LogInformation("Sending generation completion notification for document {DocumentId}", document.Id);
        await notificationService.SendDocumentGenerationNotificationAsync(document);
    }

    [Function("HandleDocumentStatusChangeActivity")]
    public async Task HandleDocumentStatusChangeActivity([ActivityTrigger] dynamic input)
    {
        var document = JsonConvert.DeserializeObject<DocumentResponse>(input.Document.ToString());
        var newStatus = (DocumentStatus)Enum.Parse(typeof(DocumentStatus), input.NewStatus.ToString());

        logger.LogInformation($"Document {document.Id} status changed to {newStatus}");

        // Handle different status changes (notifications, integrations, etc.)
        switch (newStatus)
        {
            case DocumentStatus.Submitted:
                // Send submission confirmation
                logger.LogInformation($"Document {document.Id} submitted");
                break;
            case DocumentStatus.Approved:
                // Send approval notification
                logger.LogInformation($"Document {document.Id} approved");
                break;
            case DocumentStatus.Rejected:
                // Send rejection notification with feedback
                logger.LogInformation($"Document {document.Id} rejected");
                break;
        }

        // Send status change notification
        await notificationService.SendDocumentStatusChangeNotificationAsync(document, newStatus.ToString());
    }
}