namespace Legal_IA.Services;

public interface IEmailService
{
    /// <summary>
    ///     Sends the verification email using the template and logs the process.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="firstName">Recipient's first name.</param>
    /// <param name="verificationLink">Verification link for account activation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SendVerificationEmailAsync(string to, string firstName, string verificationLink,
        CancellationToken cancellationToken = default);
}