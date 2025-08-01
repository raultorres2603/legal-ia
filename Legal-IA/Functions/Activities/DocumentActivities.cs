using FluentValidation;
using Legal_IA.DTOs;
using Legal_IA.Interfaces.Services;
using Legal_IA.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Legal_IA.Functions.Activities;

/// <summary>
///     Document-related activity functions
/// </summary>
public class DocumentActivities(
    ILogger<DocumentActivities> logger,
    IDocumentService documentService,
    IFileStorageService fileStorageService)
{
    [Function("ValidateDocumentActivity")]
    public Task ValidateDocumentActivity([ActivityTrigger] CreateDocumentRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ValidationException("Document title is required");

        if (request.UserId == Guid.Empty)
            throw new ValidationException("Valid User ID is required");

        return Task.CompletedTask;
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

        logger.LogInformation($"Generating content for document {document.Id} of type {document.Type}");

        // Simple template-based content generation without AI
        var generatedContent = GenerateSimpleDocumentContent(document, parameters!);

        return await Task.FromResult(generatedContent);
    }

    private static string GenerateSimpleDocumentContent(DocumentResponse document,
        Dictionary<string, object> parameters)
    {
        return $@"
DOCUMENT: {document.Title}

Document ID: {document.Id}
Type: {document.Type}
Created: {document.CreatedAt:dd/MM/yyyy}
Status: {document.Status}

Description:
{document.Description}

Amount: {document.Amount:F2} {document.Currency}
Quarter: {document.Quarter}
Year: {document.Year}

Parameters:
{string.Join("\n", parameters.Select(p => $"- {p.Key}: {p.Value}"))}

Generated on: {DateTime.Now:dd/MM/yyyy HH:mm:ss}
";
    }

    [Function("SaveGeneratedDocumentActivity")]
    public async Task<string> SaveGeneratedDocumentActivity([ActivityTrigger] dynamic input)
    {
        var documentId = Guid.Parse(input.DocumentId.ToString());
        var content = input.Content.ToString();

        var fileName = $"document_{documentId}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.pdf";
        var filePath = await fileStorageService.SaveDocumentAsync(content, fileName, "application/pdf");

        logger.LogInformation($"Document saved to {filePath}");
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

        if (document != null) logger.LogInformation($"Document {documentId} finalized with status {status}");

        return document;
    }

    [Function("InitializeTemplateActivity")]
    public Task InitializeTemplateActivity([ActivityTrigger] Guid documentId)
    {
        logger.LogInformation("Initializing template for document {DocumentId}", documentId);
        // Here you would set up template-specific configurations
        return Task.CompletedTask;
    }

    [Function("ProcessSingleDocumentActivity")]
    public async Task<DocumentResponse> ProcessSingleDocumentActivity([ActivityTrigger] Guid documentId)
    {
        var document = await documentService.GetDocumentByIdAsync(documentId);
        if (document == null)
            throw new ArgumentException($"Document {documentId} not found");

        logger.LogInformation("Processing document {DocumentId}", documentId);

        return document;
    }
}