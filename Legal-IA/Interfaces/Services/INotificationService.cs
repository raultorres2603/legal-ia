// ...existing code...

using Legal_IA.DTOs;

namespace Legal_IA.Interfaces.Services;

public interface INotificationService
{
    Task SendWelcomeNotificationAsync(UserResponse user);
    Task SendUserUpdateNotificationAsync(UserResponse user);

    Task SendEmailVerificationAsync(string email, string firstName, string verificationToken);
// ...existing code...
}