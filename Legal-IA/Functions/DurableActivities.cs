using System.Net.Mail;
using FluentValidation;
using Legal_IA.DTOs;
using Legal_IA.Interfaces.Services;
using Legal_IA.Models;
using Legal_IA.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Legal_IA.Functions;

public class DurableActivities(
    ILogger<DurableActivities> logger,
    IUserService userService,
    IDocumentService documentService)
{
    // User Activities
    [Function("ValidateUserActivity")]
    public async Task ValidateUserActivity([ActivityTrigger] CreateUserRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            throw new ValidationException("Email is required");

        if (string.IsNullOrWhiteSpace(request.DNI))
            throw new ValidationException("DNI is required");

        if (string.IsNullOrWhiteSpace(request.CIF))
            throw new ValidationException("CIF is required");

        // Check if user already exists
        if (await userService.UserExistsByEmailAsync(request.Email))
            throw new ValidationException("User with this email already exists");

        if (await userService.UserExistsByDNIAsync(request.DNI))
            throw new ValidationException("User with this DNI already exists");
    }

    [Function("CreateUserActivity")]
    public async Task<UserResponse> CreateUserActivity([ActivityTrigger] CreateUserRequest request)
    {
        return await userService.CreateUserAsync(request);
    }

    [Function("ValidateUserUpdateActivity")]
    public Task ValidateUserUpdateActivity([ActivityTrigger] UpdateUserRequest request)
    {
        // Add validation logic for update requests
        if (!string.IsNullOrWhiteSpace(request.Email) && !IsValidEmail(request.Email))
            throw new ValidationException("Invalid email format");

        return Task.CompletedTask;
    }

    [Function("UpdateUserActivity")]
    public async Task<UserResponse?> UpdateUserActivity([ActivityTrigger] dynamic input)
    {
        var userId = Guid.Parse(input.UserId.ToString());
        var updateRequest = JsonConvert.DeserializeObject<UpdateUserRequest>(input.UpdateRequest.ToString());

        return await userService.UpdateUserAsync(userId, updateRequest!);
    }

    [Function("SendWelcomeNotificationActivity")]
    public Task SendWelcomeNotificationActivity([ActivityTrigger] UserResponse user)
    {
        logger.LogInformation("Sending welcome notification to user {UserId} at {Email}", user.Id, user.Email);
        // Here you would integrate with your notification service (email, SMS, etc.)
        return Task.CompletedTask;
    }

    [Function("SendUserUpdateNotificationActivity")]
    public Task SendUserUpdateNotificationActivity([ActivityTrigger] UserResponse user)
    {
        logger.LogInformation("Sending update notification to user {UserId}", user.Id);
        // Here you would integrate with your notification service
        return Task.CompletedTask;
    }

    // Document Activities
    [Function("ValidateDocumentActivity")]
    public Task ValidateDocumentActivity([ActivityTrigger] CreateDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Document title is required");

        if (request.UserId == Guid.Empty)
            throw new ValidationException("Valid User ID is required");

        return Task.CompletedTask;
    }

    [Function("VerifyUserExistsActivity")]
    public async Task VerifyUserExistsActivity([ActivityTrigger] Guid userId)
    {
        var user = await userService.GetUserByIdAsync(userId);
        if (user == null)
            throw new ValidationException($"User {userId} not found");
    }

    [Function("CreateDocumentActivity")]
    public async Task<DocumentResponse> CreateDocumentActivity([ActivityTrigger] CreateDocumentRequest request)
    {
        return await documentService.CreateDocumentAsync(request);
    }

    [Function("ValidateDocumentUpdateActivity")]
    public Task ValidateDocumentUpdateActivity([ActivityTrigger] UpdateDocumentRequest request)
    {
        // Add validation logic for document updates
        return Task.CompletedTask;
    }

    [Function("UpdateDocumentActivity")]
    public async Task<DocumentResponse?> UpdateDocumentActivity([ActivityTrigger] dynamic input)
    {
        var documentId = Guid.Parse(input.DocumentId.ToString());
        var updateRequest = JsonConvert.DeserializeObject<UpdateDocumentRequest>(input.UpdateRequest.ToString());

        return await documentService.UpdateDocumentAsync(documentId, updateRequest!);
    }

    [Function("GetDocumentActivity")]
    public async Task<DocumentResponse?> GetDocumentActivity([ActivityTrigger] Guid documentId)
    {
        return await documentService.GetDocumentByIdAsync(documentId);
    }

    [Function("UpdateDocumentStatusActivity")]
    public async Task<DocumentResponse?> UpdateDocumentStatusActivity([ActivityTrigger] dynamic input)
    {
        var documentId = Guid.Parse(input.DocumentId.ToString());
        var status = (DocumentStatus)Enum.Parse(typeof(DocumentStatus), input.Status.ToString());

        return await documentService.UpdateDocumentStatusAsync(documentId, status);
    }

    [Function("GenerateDocumentContentActivity")]
    public async Task<string> GenerateDocumentContentActivity([ActivityTrigger] dynamic input)
    {
        var document = JsonConvert.DeserializeObject<DocumentResponse>(input.Document.ToString());
        var parameters = JsonConvert.DeserializeObject<Dictionary<string, object>>(input.Parameters.ToString());

        LoggerExtensions.LogInformation(logger, "Generating content for document {DocumentId} of type {DocumentType}",
            document!.Id, document.Type);

        // This is where you would integrate with your AI Agent
        // For now, we'll return a placeholder
        var generatedContent = await GenerateContentBasedOnType(document, parameters!);

        return generatedContent;
    }

    [Function("SaveGeneratedDocumentActivity")]
    public async Task<string> SaveGeneratedDocumentActivity([ActivityTrigger] dynamic input)
    {
        var documentId = Guid.Parse(input.DocumentId.ToString());
        var content = input.Content.ToString();

        // Generate file path and save the document
        var fileName = $"document_{documentId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
        var filePath = Path.Combine("generated_documents", fileName);

        // Create directory if it doesn't exist
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory)) Directory.CreateDirectory(directory);

        // Save content to file (in a real scenario, you might save to Azure Blob Storage)
        await File.WriteAllTextAsync(filePath, content);

        logger.LogInformation("Document saved to {FilePath}", filePath);
        return filePath;
    }

    [Function("FinalizeDocumentActivity")]
    public async Task<DocumentResponse?> FinalizeDocumentActivity([ActivityTrigger] dynamic input)
    {
        var documentId = Guid.Parse(input.DocumentId.ToString());
        var filePath = input.FilePath.ToString();
        var status = (DocumentStatus)Enum.Parse(typeof(DocumentStatus), input.Status.ToString());

        var updateRequest = new UpdateDocumentRequest
        {
            Status = status
        };

        var document = await documentService.UpdateDocumentAsync(documentId, updateRequest);

        if (document != null)
            // Update file information in database (you might need to extend the service for this)
            logger.LogInformation("Document finalized");

        return document;
    }

    [Function("InitializeTemplateActivity")]
    public Task InitializeTemplateActivity([ActivityTrigger] Guid documentId)
    {
        logger.LogInformation("Initializing template for document {DocumentId}", documentId);
        // Here you would set up template-specific configurations
        return Task.CompletedTask;
    }

    [Function("HandleDocumentStatusChangeActivity")]
    public Task HandleDocumentStatusChangeActivity([ActivityTrigger] dynamic input)
    {
        var document = JsonConvert.DeserializeObject<DocumentResponse>(input.Document.ToString());
        var newStatus = (DocumentStatus)Enum.Parse(typeof(DocumentStatus), input.NewStatus.ToString());

        logger.LogInformation($"Handling status change for document {document.Id} to {newStatus}");

        // Handle different status changes (notifications, integrations, etc.)
        switch (newStatus)
        {
            case DocumentStatus.Submitted:
                // Send submission confirmation
                break;
            case DocumentStatus.Approved:
                // Send approval notification
                break;
            case DocumentStatus.Rejected:
                // Send rejection notification with feedback
                break;
        }

        return Task.CompletedTask;
    }

    [Function("SendDocumentGenerationNotificationActivity")]
    public Task SendDocumentGenerationNotificationActivity([ActivityTrigger] DocumentResponse document)
    {
        logger.LogInformation("Sending generation completion notification for document {DocumentId}", document.Id);
        // Here you would send notifications to the user about document completion
        return Task.CompletedTask;
    }

    [Function("ProcessSingleDocumentActivity")]
    public async Task<DocumentResponse> ProcessSingleDocumentActivity([ActivityTrigger] Guid documentId)
    {
        var document = await documentService.GetDocumentByIdAsync(documentId);
        if (document == null)
            throw new ArgumentException($"Document {documentId} not found");

        // Process the document (generate, validate, etc.)
        logger.LogInformation("Processing document {DocumentId}", documentId);

        return document;
    }

    // Helper methods
    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private async Task<string> GenerateContentBasedOnType(DocumentResponse document,
        Dictionary<string, object> parameters)
    {
        // This method would integrate with your AI Agent
        // For now, return template content based on document type

        return document.Type switch
        {
            DocumentType.Invoice => await GenerateInvoiceContent(document, parameters),
            DocumentType.VATReturn => await GenerateVATReturnContent(document, parameters),
            DocumentType.IRPFReturn => await GenerateIRPFReturnContent(document, parameters),
            DocumentType.SocialSecurityForm => await GenerateSocialSecurityFormContent(document, parameters),
            DocumentType.ExpenseReport => await GenerateExpenseReportContent(document, parameters),
            _ => $"Generated content for {document.Type} document: {document.Title}"
        };
    }

    private Task<string> GenerateInvoiceContent(DocumentResponse document, Dictionary<string, object> parameters)
    {
        var content = $@"
FACTURA

Número: INV-{document.Id:N}[..8]
Fecha: {DateTime.Now:dd/MM/yyyy}

Datos del Autónomo:
{GetParameterValue(parameters, "businessName", "Nombre del Negocio")}
CIF: {GetParameterValue(parameters, "cif", "B12345678")}
Dirección: {GetParameterValue(parameters, "address", "Dirección del negocio")}

Cliente:
{GetParameterValue(parameters, "clientName", "Nombre del Cliente")}
{GetParameterValue(parameters, "clientAddress", "Dirección del Cliente")}

Descripción: {document.Description}
Importe: {document.Amount:C} {document.Currency}
IVA (21%): {(document.Amount ?? 0) * 0.21m:C}
Total: {(document.Amount ?? 0) * 1.21m:C}
";
        return Task.FromResult(content);
    }

    private Task<string> GenerateVATReturnContent(DocumentResponse document, Dictionary<string, object> parameters)
    {
        var content = $@"
DECLARACIÓN DE IVA - TRIMESTRE {document.Quarter}/{document.Year}

Identificación del Declarante:
CIF: {GetParameterValue(parameters, "cif", "12345678Z")}
Nombre/Razón Social: {GetParameterValue(parameters, "businessName", "Autónomo")}

Período: {document.Quarter}º Trimestre {document.Year}

Operaciones realizadas:
- Base imponible: {GetParameterValue(parameters, "taxableBase", "0,00")} €
- Cuota devengada: {GetParameterValue(parameters, "outputVAT", "0,00")} €
- Cuota soportada: {GetParameterValue(parameters, "inputVAT", "0,00")} €

Resultado: {GetParameterValue(parameters, "result", "0,00")} €
";
        return Task.FromResult(content);
    }

    private Task<string> GenerateIRPFReturnContent(DocumentResponse document, Dictionary<string, object> parameters)
    {
        var content = $@"
DECLARACIÓN DE IRPF - AÑO {document.Year}

Datos del Declarante:
DNI: {GetParameterValue(parameters, "dni", "12345678Z")}
Nombre: {GetParameterValue(parameters, "fullName", "Nombre Completo")}

Actividad Económica:
Ingresos: {GetParameterValue(parameters, "income", "0,00")} €
Gastos deducibles: {GetParameterValue(parameters, "expenses", "0,00")} €
Rendimiento neto: {decimal.Parse(GetParameterValue(parameters, "income", "0")) - decimal.Parse(GetParameterValue(parameters, "expenses", "0")):F2} €

Retenciones practicadas: {GetParameterValue(parameters, "retentions", "0,00")} €
";
        return Task.FromResult(content);
    }

    private Task<string> GenerateSocialSecurityFormContent(DocumentResponse document,
        Dictionary<string, object> parameters)
    {
        var content = $@"
FORMULARIO SEGURIDAD SOCIAL

Número de Afiliación: {GetParameterValue(parameters, "socialSecurityNumber", "123456789012")}
Nombre: {GetParameterValue(parameters, "fullName", "Nombre Completo")}

Período: {document.Quarter}º Trimestre {document.Year}

Base de cotización: {GetParameterValue(parameters, "contributionBase", "0,00")} €
Cuota a ingresar: {GetParameterValue(parameters, "contributionAmount", "0,00")} €

Fecha de vencimiento: {GetParameterValue(parameters, "dueDate", DateTime.Now.AddDays(30).ToString("dd/MM/yyyy"))}
";
        return Task.FromResult(content);
    }

    private Task<string> GenerateExpenseReportContent(DocumentResponse document, Dictionary<string, object> parameters)
    {
        var content = $@"
INFORME DE GASTOS

Período: {GetParameterValue(parameters, "period", $"{document.Quarter}º Trimestre {document.Year}")}
Autónomo: {GetParameterValue(parameters, "businessName", "Nombre del Autónomo")}

Resumen de gastos:
- Material de oficina: {GetParameterValue(parameters, "officeSupplies", "0,00")} €
- Transporte: {GetParameterValue(parameters, "transport", "0,00")} €
- Comunicaciones: {GetParameterValue(parameters, "communications", "0,00")} €
- Otros gastos: {GetParameterValue(parameters, "otherExpenses", "0,00")} €

Total gastos: {document.Amount:F2} €
";
        return Task.FromResult(content);
    }

    private static string GetParameterValue(Dictionary<string, object> parameters, string key, string defaultValue)
    {
        return parameters.TryGetValue(key, out var value) ? value?.ToString() ?? defaultValue : defaultValue;
    }
}