using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Legal_IA.Services;

public class EmailService(IConfiguration configuration, ILogger<EmailService> logger) : IEmailService
{
    private readonly string _sendGridApiKey = configuration["SendGrid:ApiKey"] ?? throw new ArgumentNullException("SendGrid:ApiKey not configured");
    private readonly string _fromEmail = configuration["SendGrid:FromAddress"] ?? throw new ArgumentNullException("SendGrid:FromAddress not configured");
    private readonly string _fromName = configuration["SendGrid:FromName"] ?? "LegalIA";

    /// <summary>
    ///     Sends the verification email using the template and logs the process.
    /// </summary>
    /// <param name="to">Recipient email address.</param>
    /// <param name="firstName">Recipient's first name.</param>
    /// <param name="verificationLink">Verification link for account activation.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public async Task SendVerificationEmailAsync(string to, string firstName, string verificationLink,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("[EmailService] Preparing to send verification email to {Email}.", to);
        var subject = "Verifica tu cuenta de LegalIA";
        var htmlBody = GetVerificationEmailBody(firstName, verificationLink);
        await SendEmailAsync(to, subject, htmlBody, cancellationToken);
        logger.LogInformation("[EmailService] Verification email sent to {Email}.", to);
    }

    // Generic method to send an email
    private async Task SendEmailAsync(string to, string subject, string htmlBody,
        CancellationToken cancellationToken = default)
    {
        var client = new SendGridClient(_sendGridApiKey);
        var from = new EmailAddress(_fromEmail, _fromName);
        var toEmail = new EmailAddress(to);
        var msg = MailHelper.CreateSingleEmail(from, toEmail, subject, null, htmlBody);
        var response = await client.SendEmailAsync(msg, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Body.ReadAsStringAsync(cancellationToken);
            logger.LogError("SendGrid failed: {StatusCode} {Body}", response.StatusCode, body);
            throw new InvalidOperationException($"SendGrid failed: {response.StatusCode} {body}");
        }
    }

    /// <summary>
    ///     Generates the HTML body for the verification email (formal, attractive, Spanish)
    /// </summary>
    /// <param name="firstName">The recipient's first name.</param>
    /// <param name="verificationLink">The verification link for account activation.</param>
    /// <returns>HTML string for the verification email body.</returns>
    private string GetVerificationEmailBody(string firstName, string verificationLink)
    {
        return $@"
            <div style='font-family: Arial, Helvetica, sans-serif; background-color: #f7f7f7; padding: 30px;'>
                <div style='max-width: 500px; margin: auto; background: #fff; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.07); padding: 32px;'>
                    <h2 style='color: #1a237e; text-align: center;'>Bienvenido a LegalIA</h2>
                    <p style='font-size: 16px; color: #333;'>Estimado/a <b>{firstName}</b>,</p>
                    <p style='font-size: 16px; color: #333;'>
                        Gracias por registrarse en <b>LegalIA</b>.<br>
                        Para completar el proceso de activación de su cuenta, por favor haga clic en el siguiente botón:
                    </p>
                    <div style='text-align: center; margin: 32px 0;'>
                        <a href='{verificationLink}' style='background: #3949ab; color: #fff; text-decoration: none; padding: 14px 32px; border-radius: 5px; font-size: 18px; font-weight: bold; display: inline-block;'>
                            Activar Cuenta
                        </a>
                    </div>
                    <p style='font-size: 14px; color: #888;'>
                        Si usted no solicitó esta cuenta, puede ignorar este correo electrónico.
                    </p>
                    <hr style='border: none; border-top: 1px solid #eee; margin: 32px 0;'>
                    <p style='font-size: 12px; color: #bbb; text-align: center;'>
                        © {DateTime.Now.Year} LegalIA. Todos los derechos reservados.
                    </p>
                </div>
            </div>";
    }
}