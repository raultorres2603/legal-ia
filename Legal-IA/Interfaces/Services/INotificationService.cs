// ...existing code...

using Legal_IA.DTOs;

namespace Legal_IA.Interfaces.Services;

public interface INotificationService
{
    /// <summary>
    /// Sends a welcome notification to the specified user.
    /// </summary>
    /// <param name="user">The user to notify.</param>
    Task SendWelcomeNotificationAsync(UserResponse user);

    /// <summary>
    /// Sends a notification to the user when their information is updated.
    /// </summary>
    /// <param name="user">The user whose information was updated.</param>
    Task SendUserUpdateNotificationAsync(UserResponse user);

    /// <summary>
    /// Sends an email verification message to the specified email address.
    /// </summary>
    /// <param name="email">The recipient's email address.</param>
    /// <param name="firstName">The recipient's first name.</param>
    /// <param name="verificationToken">The verification token to include in the email.</param>
    Task SendEmailVerificationAsync(string email, string firstName, string verificationToken);

    // ...existing code...
 
}