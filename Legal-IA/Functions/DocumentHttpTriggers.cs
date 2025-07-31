using System.Web;
using Legal_IA.DTOs;
using Legal_IA.Interfaces.Services;
using Legal_IA.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Legal_IA.Functions;

public class DocumentHttpTriggers(ILogger<DocumentHttpTriggers> logger, IDocumentService documentService)
{
    [Function("GetDocuments")]
    public async Task<IActionResult> GetDocuments(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "documents")]
        HttpRequestData req)
    {
        try
        {
            var query = HttpUtility.ParseQueryString(req.Url.Query);
            var userIdStr = query["userId"];
            var typeStr = query["type"];
            var statusStr = query["status"];
            var searchTerm = query["search"];

            if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var userId))
            {
                var documents = await documentService.GetDocumentsByUserIdAsync(userId);
                return new OkObjectResult(documents);
            }

            if (!string.IsNullOrEmpty(typeStr) && Enum.TryParse<DocumentType>(typeStr, out var type))
            {
                var documents = await documentService.GetDocumentsByTypeAsync(type);
                return new OkObjectResult(documents);
            }

            if (!string.IsNullOrEmpty(statusStr) && Enum.TryParse<DocumentStatus>(statusStr, out var status))
            {
                var documents = await documentService.GetDocumentsByStatusAsync(status);
                return new OkObjectResult(documents);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                Guid? searchUserId = null;
                if (!string.IsNullOrEmpty(userIdStr) && Guid.TryParse(userIdStr, out var sUserId))
                    searchUserId = sUserId;

                var documents = await documentService.SearchDocumentsAsync(searchTerm, searchUserId);
                return new OkObjectResult(documents);
            }

            return new BadRequestObjectResult("Please specify userId, type, status, or search parameter");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting documents");
            return new StatusCodeResult(500);
        }
    }

    [Function("GetDocument")]
    public async Task<IActionResult> GetDocument(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "documents/{id}")]
        HttpRequestData req,
        string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var documentId))
                return new BadRequestObjectResult("Invalid document ID format");

            var document = await documentService.GetDocumentByIdAsync(documentId);
            if (document == null)
                return new NotFoundResult();

            return new OkObjectResult(document);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting document {DocumentId}", id);
            return new StatusCodeResult(500);
        }
    }

    [Function("CreateDocument")]
    public async Task<IActionResult> CreateDocument(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "documents")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var createRequest = JsonConvert.DeserializeObject<CreateDocumentRequest>(requestBody);

            if (createRequest == null)
                return new BadRequestObjectResult("Invalid request body");

            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "DocumentCreationOrchestrator", createRequest);

            return new AcceptedResult($"/api/orchestrations/{instanceId}", new { instanceId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating document");
            return new StatusCodeResult(500);
        }
    }

    [Function("UpdateDocument")]
    public async Task<IActionResult> UpdateDocument(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "documents/{id}")]
        HttpRequestData req,
        string id,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            if (!Guid.TryParse(id, out var documentId))
                return new BadRequestObjectResult("Invalid document ID format");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updateRequest = JsonConvert.DeserializeObject<UpdateDocumentRequest>(requestBody);

            if (updateRequest == null)
                return new BadRequestObjectResult("Invalid request body");

            var updateData = new { DocumentId = documentId, UpdateRequest = updateRequest };
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "DocumentUpdateOrchestrator", updateData);

            return new AcceptedResult($"/api/orchestrations/{instanceId}", new { instanceId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating document {DocumentId}", id);
            return new StatusCodeResult(500);
        }
    }

    [Function("GenerateDocument")]
    public async Task<IActionResult> GenerateDocument(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "documents/{id}/generate")]
        HttpRequestData req,
        string id,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            if (!Guid.TryParse(id, out var documentId))
                return new BadRequestObjectResult("Invalid document ID format");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var generationRequest = JsonConvert.DeserializeObject<DocumentGenerationRequest>(requestBody)
                                    ?? new DocumentGenerationRequest { DocumentId = documentId };

            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "DocumentGenerationOrchestrator", generationRequest);

            return new AcceptedResult($"/api/orchestrations/{instanceId}", new { instanceId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating document {DocumentId}", id);
            return new StatusCodeResult(500);
        }
    }

    [Function("GetTemplates")]
    public async Task<IActionResult> GetTemplates(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "documents/templates")]
        HttpRequestData req)
    {
        try
        {
            var templates = await documentService.GetTemplatesAsync();
            return new OkObjectResult(templates);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting templates");
            return new StatusCodeResult(500);
        }
    }

    [Function("DeleteDocument")]
    public async Task<IActionResult> DeleteDocument(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "documents/{id}")]
        HttpRequestData req,
        string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var documentId))
                return new BadRequestObjectResult("Invalid document ID format");

            var result = await documentService.DeleteDocumentAsync(documentId);
            if (!result)
                return new NotFoundResult();

            return new NoContentResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting document {DocumentId}", id);
            return new StatusCodeResult(500);
        }
    }
}