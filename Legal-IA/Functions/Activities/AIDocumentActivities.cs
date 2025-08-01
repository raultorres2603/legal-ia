using FluentValidation;
using Legal_IA.DTOs;
using Legal_IA.Interfaces.Services;
using Legal_IA.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Legal_IA.Functions.Activities;

/// <summary>
///     AI Document generation-related activity functions
/// </summary>
public class AIDocumentActivities(
    ILogger<AIDocumentActivities> logger,
    IAIDocumentGenerationService aiDocumentService,
    IDocumentService documentService,
    IFileStorageService fileStorageService,
    IValidator<GenerateDocumentRequest> generateValidator,
    IValidator<RegenerateDocumentRequest> regenerateValidator)
{
    [Function("ValidateGenerateDocumentActivity")]
    public async Task ValidateGenerateDocumentActivity([ActivityTrigger] GenerateDocumentRequest request)
    {
        logger.LogInformation("Validating AI document generation request for user {UserId}", request.UserId);

        var validationResult = await generateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException($"Validation failed: {errors}");
        }
    }

    [Function("GenerateAIDocumentActivity")]
    public async Task<DocumentResponse> GenerateAIDocumentActivity([ActivityTrigger] GenerateDocumentRequest request)
    {
        logger.LogInformation("Generating AI document for user {UserId}, type {DocumentType}",
            request.UserId, request.DocumentType);

        return await aiDocumentService.GenerateDocumentAsync(request);
    }

    [Function("ValidateRegenerateDocumentActivity")]
    public async Task ValidateRegenerateDocumentActivity([ActivityTrigger] RegenerateDocumentRequest request)
    {
        logger.LogInformation("Validating AI document regeneration request for document {DocumentId}",
            request.DocumentId);

        var validationResult = await regenerateValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException($"Validation failed: {errors}");
        }
    }

    [Function("VerifyDocumentExistsActivity")]
    public async Task<DocumentResponse> VerifyDocumentExistsActivity([ActivityTrigger] Guid documentId)
    {
        logger.LogInformation("Verifying document exists: {DocumentId}", documentId);

        var document = await documentService.GetDocumentByIdAsync(documentId);
        if (document == null) throw new InvalidOperationException($"Document with ID {documentId} not found");

        return document;
    }

    [Function("RegenerateAIDocumentActivity")]
    public async Task<DocumentResponse> RegenerateAIDocumentActivity([ActivityTrigger] dynamic input)
    {
        var existingDocument = JsonConvert.DeserializeObject<DocumentResponse>(input.ExistingDocument.ToString());
        var regenerateRequest =
            JsonConvert.DeserializeObject<RegenerateDocumentRequest>(input.RegenerateRequest.ToString());

        logger.LogInformation($"Regenerating AI document {regenerateRequest.DocumentId}");

        // Create new generation request based on existing document
        var generateRequest = new GenerateDocumentRequest
        {
            UserId = existingDocument!.UserId,
            DocumentType = existingDocument.Type,
            Title = existingDocument.Title,
            Description = existingDocument.Description,
            UserPrompts = regenerateRequest.UpdatedPrompts,
            AdditionalContext = regenerateRequest.UpdatedContext ?? string.Empty,
            Tags = existingDocument.Tags,
            Amount = existingDocument.Amount,
            Currency = existingDocument.Currency,
            Quarter = existingDocument.Quarter,
            Year = existingDocument.Year
        };

        // Generate new document
        var newDocument = await aiDocumentService.GenerateDocumentAsync(generateRequest);

        // Delete the old document
        await documentService.DeleteDocumentAsync(regenerateRequest.DocumentId);

        return newDocument;
    }

    [Function("GetDocumentStatusActivity")]
    public async Task<DocumentResponse> GetDocumentStatusActivity([ActivityTrigger] Guid documentId)
    {
        logger.LogInformation("Getting document status for {DocumentId}", documentId);

        var document = await documentService.GetDocumentByIdAsync(documentId);
        if (document == null) throw new InvalidOperationException($"Document with ID {documentId} not found");

        return document;
    }

    [Function("DownloadDocumentActivity")]
    public async Task<byte[]> DownloadDocumentActivity([ActivityTrigger] DocumentResponse document)
    {
        logger.LogInformation("Downloading document {DocumentId}", document.Id);

        if (string.IsNullOrEmpty(document.FileName) || document.Status != DocumentStatus.Generated)
            throw new InvalidOperationException("Document not yet generated or file not available");

        // Get file from blob storage
        var filePath = $"documents/{document.FileName}";
        return await fileStorageService.GetDocumentBytesAsync(filePath);
    }

    [Function("ValidateDocumentForDownloadActivity")]
    public Task ValidateDocumentForDownloadActivity([ActivityTrigger] DocumentResponse document)
    {
        logger.LogInformation("Validating document for download: {DocumentId}", document.Id);

        if (string.IsNullOrEmpty(document.FileName)) throw new InvalidOperationException("Document file not available");

        if (document.Status != DocumentStatus.Generated)
            throw new InvalidOperationException($"Document status is {document.Status}, expected Generated");

        return Task.CompletedTask;
    }

    [Function("NotifyDocumentGenerationActivity")]
    public async Task NotifyDocumentGenerationActivity([ActivityTrigger] DocumentResponse document)
    {
        logger.LogInformation("Sending notification for document generation: {DocumentId}", document.Id);

        // This would typically send notifications via email, SMS, etc.
        // For now, we'll just log the completion
        logger.LogInformation("AI document generation completed successfully for document {DocumentId}, user {UserId}",
            document.Id, document.UserId);
    }

    [Function("LogAIDocumentGenerationActivity")]
    public Task LogAIDocumentGenerationActivity([ActivityTrigger] dynamic input)
    {
        var documentId = Guid.Parse(input.DocumentId.ToString());
        var userId = Guid.Parse(input.UserId.ToString());
        var documentType = input.DocumentType.ToString();
        var promptCount = int.Parse(input.PromptCount.ToString());

        logger.LogInformation(
            $"AI Document Generation - DocumentId: {documentId}, UserId: {userId}, Type: {documentType}, Prompts: {promptCount}");

        return Task.CompletedTask;
    }
}