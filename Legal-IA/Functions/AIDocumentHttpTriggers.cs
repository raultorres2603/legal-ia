using System.Net;
using Legal_IA.DTOs;
using Legal_IA.Interfaces.Services;
using Legal_IA.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Legal_IA.Functions;

/// <summary>
///     HTTP triggers for AI document generation operations
/// </summary>
public class AIDocumentHttpTriggers(
    IAIDocumentGenerationService aiDocumentService,
    IDocumentService documentService,
    IFileStorageService fileStorageService,
    ILogger<AIDocumentHttpTriggers> logger)
{
    /// <summary>
    ///     Generate a new document using AI based on user prompts
    /// </summary>
    [Function("GenerateDocument")]
    public async Task<HttpResponseData> GenerateDocumentAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ai/documents/generate")] HttpRequestData req)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<GenerateDocumentRequest>(requestBody);

            if (request == null)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid request body");
                return badRequest;
            }

            // Validate required fields
            if (request.UserId == Guid.Empty || request.UserPrompts.Count == 0)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("UserId and UserPrompts are required");
                return badRequest;
            }

            logger.LogInformation("Generating AI document for user {UserId} with {PromptCount} prompts", 
                request.UserId, request.UserPrompts.Count);

            var documentResponse = await aiDocumentService.GenerateDocumentAsync(request);

            var response = req.CreateResponse(HttpStatusCode.Created);
            await response.WriteStringAsync(JsonConvert.SerializeObject(new GenerateDocumentResponse
            {
                DocumentId = documentResponse.Id,
                Title = documentResponse.Title,
                Type = documentResponse.Type,
                Status = documentResponse.Status,
                BlobPath = documentResponse.FileName, // This will contain the blob path
                FileName = documentResponse.FileName,
                FileSize = documentResponse.FileSize,
                GeneratedAt = documentResponse.GeneratedAt ?? DateTime.UtcNow,
                Message = "Document generated successfully"
            }));

            response.Headers.Add("Content-Type", "application/json");
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating AI document");
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error generating document: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    ///     Regenerate an existing document with updated prompts
    /// </summary>
    [Function("RegenerateDocument")]
    public async Task<HttpResponseData> RegenerateDocumentAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ai/documents/{documentId}/regenerate")] 
        HttpRequestData req, string documentId)
    {
        try
        {
            if (!Guid.TryParse(documentId, out var docId))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid document ID");
                return badRequest;
            }

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<RegenerateDocumentRequest>(requestBody);

            if (request == null || request.UpdatedPrompts.Count == 0)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("UpdatedPrompts are required");
                return badRequest;
            }

            // Get existing document
            var existingDocument = await documentService.GetDocumentByIdAsync(docId);
            if (existingDocument == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Document not found");
                return notFound;
            }

            logger.LogInformation("Regenerating document {DocumentId} with {PromptCount} updated prompts", 
                docId, request.UpdatedPrompts.Count);

            // Create new generation request based on existing document
            var generateRequest = new GenerateDocumentRequest
            {
                UserId = existingDocument.UserId,
                DocumentType = existingDocument.Type,
                Title = existingDocument.Title,
                Description = existingDocument.Description,
                UserPrompts = request.UpdatedPrompts,
                AdditionalContext = request.UpdatedContext ?? string.Empty,
                Tags = existingDocument.Tags,
                Amount = existingDocument.Amount,
                Currency = existingDocument.Currency,
                Quarter = existingDocument.Quarter,
                Year = existingDocument.Year
            };

            var documentResponse = await aiDocumentService.GenerateDocumentAsync(generateRequest);

            // Delete the old document if regeneration was successful
            await documentService.DeleteDocumentAsync(docId);

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(JsonConvert.SerializeObject(new GenerateDocumentResponse
            {
                DocumentId = documentResponse.Id,
                Title = documentResponse.Title,
                Type = documentResponse.Type,
                Status = documentResponse.Status,
                BlobPath = documentResponse.FileName,
                FileName = documentResponse.FileName,
                FileSize = documentResponse.FileSize,
                GeneratedAt = documentResponse.GeneratedAt ?? DateTime.UtcNow,
                Message = "Document regenerated successfully"
            }));

            response.Headers.Add("Content-Type", "application/json");
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error regenerating document {DocumentId}", documentId);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error regenerating document: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    ///     Download a generated PDF document
    /// </summary>
    [Function("DownloadDocument")]
    public async Task<HttpResponseData> DownloadDocumentAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ai/documents/{documentId}/download")] 
        HttpRequestData req, string documentId)
    {
        try
        {
            if (!Guid.TryParse(documentId, out var docId))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid document ID");
                return badRequest;
            }

            var document = await documentService.GetDocumentByIdAsync(docId);
            if (document == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Document not found");
                return notFound;
            }

            if (string.IsNullOrEmpty(document.FileName) || document.Status != DocumentStatus.Generated)
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Document not yet generated or file not available");
                return badRequest;
            }

            // Get file from blob storage
            var filePath = $"documents/{document.FileName}";
            var fileBytes = await fileStorageService.GetDocumentBytesAsync(filePath);

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/pdf");
            response.Headers.Add("Content-Disposition", $"attachment; filename=\"{document.FileName}\"");
            await response.WriteBytesAsync(fileBytes);

            logger.LogInformation("Document {DocumentId} downloaded successfully", docId);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error downloading document {DocumentId}", documentId);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error downloading document: {ex.Message}");
            return errorResponse;
        }
    }

    /// <summary>
    ///     Get AI document generation status
    /// </summary>
    [Function("GetDocumentStatus")]
    public async Task<HttpResponseData> GetDocumentStatusAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ai/documents/{documentId}/status")] 
        HttpRequestData req, string documentId)
    {
        try
        {
            if (!Guid.TryParse(documentId, out var docId))
            {
                var badRequest = req.CreateResponse(HttpStatusCode.BadRequest);
                await badRequest.WriteStringAsync("Invalid document ID");
                return badRequest;
            }

            var document = await documentService.GetDocumentByIdAsync(docId);
            if (document == null)
            {
                var notFound = req.CreateResponse(HttpStatusCode.NotFound);
                await notFound.WriteStringAsync("Document not found");
                return notFound;
            }

            var statusResponse = new
            {
                DocumentId = document.Id,
                Status = document.Status.ToString(),
                Title = document.Title,
                Type = document.Type.ToString(),
                CreatedAt = document.CreatedAt,
                UpdatedAt = document.UpdatedAt,
                GeneratedAt = document.GeneratedAt,
                FileSize = document.FileSize,
                IsReady = document.Status == DocumentStatus.Generated && !string.IsNullOrEmpty(document.FileName)
            };

            var response = req.CreateResponse(HttpStatusCode.OK);
            await response.WriteStringAsync(JsonConvert.SerializeObject(statusResponse));
            response.Headers.Add("Content-Type", "application/json");
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting document status {DocumentId}", documentId);
            var errorResponse = req.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteStringAsync($"Error getting document status: {ex.Message}");
            return errorResponse;
        }
    }
}
