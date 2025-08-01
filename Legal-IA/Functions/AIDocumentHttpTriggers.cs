using Legal_IA.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Legal_IA.Functions;

/// <summary>
///     HTTP triggers for AI document generation operations using orchestrator pattern
/// </summary>
public class AIDocumentHttpTriggers(ILogger<AIDocumentHttpTriggers> logger)
{
    /// <summary>
    ///     Generate a new document using AI based on user prompts
    /// </summary>
    [Function("GenerateAIDocument")]
    public async Task<IActionResult> GenerateAIDocumentAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ai/documents/generate")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<GenerateDocumentRequest>(requestBody);

            if (request == null) return new BadRequestObjectResult("Invalid request body");

            // Basic validation before starting orchestration
            if (request.UserId == Guid.Empty || request.UserPrompts.Count == 0)
                return new BadRequestObjectResult("UserId and UserPrompts are required");

            logger.LogInformation(
                "Starting AI document generation orchestration for user {UserId} with {PromptCount} prompts",
                request.UserId, request.UserPrompts.Count);

            // Start the orchestration
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "AIDocumentGenerationOrchestrator", request);

            logger.LogInformation("AI document generation orchestration started with instance ID: {InstanceId}",
                instanceId);

            return new AcceptedResult($"/api/ai/orchestrations/{instanceId}", new { instanceId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting AI document generation");
            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    ///     Regenerate an existing document with updated prompts
    /// </summary>
    [Function("RegenerateAIDocument")]
    public async Task<IActionResult> RegenerateAIDocumentAsync(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ai/documents/{documentId}/regenerate")]
        HttpRequestData req,
        string documentId,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            if (!Guid.TryParse(documentId, out var docId)) return new BadRequestObjectResult("Invalid document ID");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonConvert.DeserializeObject<RegenerateDocumentRequest>(requestBody);

            if (request == null || request.UpdatedPrompts.Count == 0)
                return new BadRequestObjectResult("UpdatedPrompts are required");

            // Set the document ID from the route parameter
            request.DocumentId = docId;

            logger.LogInformation(
                "Starting AI document regeneration orchestration for document {DocumentId} with {PromptCount} updated prompts",
                docId, request.UpdatedPrompts.Count);

            // Start the orchestration
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "AIDocumentRegenerationOrchestrator", request);

            logger.LogInformation("AI document regeneration orchestration started with instance ID: {InstanceId}",
                instanceId);

            return new AcceptedResult($"/api/ai/orchestrations/{instanceId}", new { instanceId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting AI document regeneration for document {DocumentId}", documentId);
            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    ///     Download a generated PDF document
    /// </summary>
    [Function("DownloadAIDocument")]
    public async Task<IActionResult> DownloadAIDocumentAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ai/documents/{documentId}/download")]
        HttpRequestData req,
        string documentId,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            if (!Guid.TryParse(documentId, out var docId)) return new BadRequestObjectResult("Invalid document ID");

            logger.LogInformation("Starting AI document download orchestration for document {DocumentId}", docId);

            // Start the orchestration
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "AIDocumentDownloadOrchestrator", docId);

            logger.LogInformation("AI document download orchestration started with instance ID: {InstanceId}",
                instanceId);

            // For download operations, we might want to wait for completion and return the file directly
            // But following the pattern, we'll return the orchestration reference
            return new AcceptedResult($"/api/ai/orchestrations/{instanceId}/download", new { instanceId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting AI document download for document {DocumentId}", documentId);
            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    ///     Get AI document generation status
    /// </summary>
    [Function("GetAIDocumentStatus")]
    public async Task<IActionResult> GetAIDocumentStatusAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ai/documents/{documentId}/status")]
        HttpRequestData req,
        string documentId,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            if (!Guid.TryParse(documentId, out var docId)) return new BadRequestObjectResult("Invalid document ID");

            logger.LogInformation("Starting AI document status check orchestration for document {DocumentId}", docId);

            // Start the orchestration
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "AIDocumentStatusOrchestrator", docId);

            logger.LogInformation("AI document status orchestration started with instance ID: {InstanceId}",
                instanceId);

            return new AcceptedResult($"/api/ai/orchestrations/{instanceId}/status", new { instanceId });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error starting AI document status check for document {DocumentId}", documentId);
            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    ///     Get orchestration status for AI document operations
    /// </summary>
    [Function("GetAIDocumentOrchestrationStatus")]
    public async Task<IActionResult> GetAIDocumentOrchestrationStatusAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ai/orchestrations/{instanceId}")]
        HttpRequestData req,
        string instanceId,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            logger.LogInformation("Getting orchestration status for instance {InstanceId}", instanceId);

            var status = await client.GetInstanceAsync(instanceId);
            if (status == null) return new NotFoundObjectResult($"Orchestration instance {instanceId} not found");

            var response = new
            {
                status.InstanceId,
                RuntimeStatus = status.RuntimeStatus.ToString(),
                status.CreatedAt,
                status.LastUpdatedAt,
                Input = status.SerializedInput,
                Output = status.SerializedOutput
            };

            return new OkObjectResult(response);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting orchestration status for instance {InstanceId}", instanceId);
            return new StatusCodeResult(500);
        }
    }

    /// <summary>
    ///     Get the result of a completed download orchestration
    /// </summary>
    [Function("GetAIDocumentDownloadResult")]
    public async Task<IActionResult> GetAIDocumentDownloadResultAsync(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ai/orchestrations/{instanceId}/download")]
        HttpRequestData req,
        string instanceId,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            logger.LogInformation("Getting download result for orchestration instance {InstanceId}", instanceId);

            var status = await client.GetInstanceAsync(instanceId);
            if (status == null) return new NotFoundObjectResult($"Orchestration instance {instanceId} not found");

            if (status.RuntimeStatus != OrchestrationRuntimeStatus.Completed)
                return new BadRequestObjectResult(
                    $"Orchestration is in status: {status.RuntimeStatus}. Expected: Completed");

            // Parse the output to get the file bytes
            if (string.IsNullOrEmpty(status.SerializedOutput))
                return new BadRequestObjectResult("No output available from orchestration");

            var fileBytes = JsonConvert.DeserializeObject<byte[]>(status.SerializedOutput);
            if (fileBytes == null || fileBytes.Length == 0) return new BadRequestObjectResult("No file data available");

            // Return the file as PDF
            return new FileContentResult(fileBytes, "application/pdf")
            {
                FileDownloadName = $"document_{instanceId}.pdf"
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting download result for orchestration instance {InstanceId}", instanceId);
            return new StatusCodeResult(500);
        }
    }
}