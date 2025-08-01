using Legal_IA.DTOs;
using Legal_IA.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Orchestrators;

/// <summary>
///     AI Document generation-related orchestrator functions
/// </summary>
public class AIDocumentOrchestrators
{
    private readonly ILogger<AIDocumentOrchestrators> _logger;

    public AIDocumentOrchestrators(ILogger<AIDocumentOrchestrators> logger)
    {
        _logger = logger;
    }

    [Function("AIDocumentGenerationOrchestrator")]
    public async Task<GenerateDocumentResponse> AIDocumentGenerationOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var generateRequest = context.GetInput<GenerateDocumentRequest>()!;

        try
        {
            _logger.LogInformation("Starting AI document generation orchestration for user {UserId}",
                generateRequest.UserId);

            // Step 1: Validate the generation request
            await context.CallActivityAsync("ValidateGenerateDocumentActivity", generateRequest);

            // Step 2: Verify user exists
            await context.CallActivityAsync("VerifyUserExistsActivity", generateRequest.UserId);

            // Step 3: Generate the AI document
            var documentResponse =
                await context.CallActivityAsync<DocumentResponse>("GenerateAIDocumentActivity", generateRequest);

            // Step 4: Log the generation for analytics
            var logData = new
            {
                DocumentId = documentResponse.Id,
                generateRequest.UserId,
                DocumentType = generateRequest.DocumentType.ToString(),
                PromptCount = generateRequest.UserPrompts.Count
            };
            await context.CallActivityAsync("LogAIDocumentGenerationActivity", logData);

            // Step 5: Send notification (fire and forget)
            await context.CallActivityAsync("NotifyDocumentGenerationActivity", documentResponse);

            _logger.LogInformation("AI document generation orchestration completed for document {DocumentId}",
                documentResponse.Id);

            return new GenerateDocumentResponse
            {
                DocumentId = documentResponse.Id,
                Title = documentResponse.Title,
                Type = documentResponse.Type,
                Status = documentResponse.Status,
                BlobPath = documentResponse.FileName,
                FileName = documentResponse.FileName,
                FileSize = documentResponse.FileSize,
                GeneratedAt = documentResponse.GeneratedAt ?? DateTime.UtcNow,
                Message = "Document generated successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AI document generation orchestration for user {UserId}",
                generateRequest.UserId);
            throw;
        }
    }

    [Function("AIDocumentRegenerationOrchestrator")]
    public async Task<GenerateDocumentResponse> AIDocumentRegenerationOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var regenerateRequest = context.GetInput<RegenerateDocumentRequest>()!;

        try
        {
            _logger.LogInformation("Starting AI document regeneration orchestration for document {DocumentId}",
                regenerateRequest.DocumentId);

            // Step 1: Validate the regeneration request
            await context.CallActivityAsync("ValidateRegenerateDocumentActivity", regenerateRequest);

            // Step 2: Verify the existing document exists
            var existingDocument =
                await context.CallActivityAsync<DocumentResponse>("VerifyDocumentExistsActivity",
                    regenerateRequest.DocumentId);

            // Step 3: Regenerate the document with updated prompts
            var regenerationData = new
            {
                ExistingDocument = existingDocument,
                RegenerateRequest = regenerateRequest
            };
            var newDocument =
                await context.CallActivityAsync<DocumentResponse>("RegenerateAIDocumentActivity", regenerationData);

            // Step 4: Log the regeneration for analytics
            var logData = new
            {
                DocumentId = newDocument.Id,
                newDocument.UserId,
                DocumentType = newDocument.Type.ToString(),
                PromptCount = regenerateRequest.UpdatedPrompts.Count,
                OriginalDocumentId = regenerateRequest.DocumentId
            };
            await context.CallActivityAsync("LogAIDocumentGenerationActivity", logData);

            // Step 5: Send notification
            await context.CallActivityAsync("NotifyDocumentGenerationActivity", newDocument);

            _logger.LogInformation("AI document regeneration orchestration completed. New document {DocumentId}",
                newDocument.Id);

            return new GenerateDocumentResponse
            {
                DocumentId = newDocument.Id,
                Title = newDocument.Title,
                Type = newDocument.Type,
                Status = newDocument.Status,
                BlobPath = newDocument.FileName,
                FileName = newDocument.FileName,
                FileSize = newDocument.FileSize,
                GeneratedAt = newDocument.GeneratedAt ?? DateTime.UtcNow,
                Message = "Document regenerated successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AI document regeneration orchestration for document {DocumentId}",
                regenerateRequest.DocumentId);
            throw;
        }
    }

    [Function("AIDocumentDownloadOrchestrator")]
    public async Task<byte[]> AIDocumentDownloadOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var documentId = context.GetInput<Guid>();

        try
        {
            _logger.LogInformation("Starting AI document download orchestration for document {DocumentId}", documentId);

            // Step 1: Verify document exists and get its details
            var document =
                await context.CallActivityAsync<DocumentResponse>("VerifyDocumentExistsActivity", documentId);

            // Step 2: Validate document is ready for download
            await context.CallActivityAsync("ValidateDocumentForDownloadActivity", document);

            // Step 3: Download the document from blob storage
            var fileBytes = await context.CallActivityAsync<byte[]>("DownloadDocumentActivity", document);

            _logger.LogInformation("AI document download orchestration completed for document {DocumentId}",
                documentId);

            return fileBytes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in AI document download orchestration for document {DocumentId}", documentId);
            throw;
        }
    }

    [Function("AIDocumentStatusOrchestrator")]
    public async Task<object> AIDocumentStatusOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var documentId = context.GetInput<Guid>();

        try
        {
            _logger.LogInformation("Getting AI document status for document {DocumentId}", documentId);

            // Get document status
            var document = await context.CallActivityAsync<DocumentResponse>("GetDocumentStatusActivity", documentId);

            return new
            {
                DocumentId = document.Id,
                Status = document.Status.ToString(),
                document.Title,
                Type = document.Type.ToString(),
                document.CreatedAt,
                document.UpdatedAt,
                document.GeneratedAt,
                document.FileSize,
                IsReady = document.Status == DocumentStatus.Generated && !string.IsNullOrEmpty(document.FileName)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI document status for document {DocumentId}", documentId);
            throw;
        }
    }
}