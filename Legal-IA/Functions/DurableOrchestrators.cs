using Legal_IA.DTOs;
using Legal_IA.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Legal_IA.Functions;

public class DurableOrchestrators(ILogger<DurableOrchestrators> logger)
{
    [Function("UserCreationOrchestrator")]
    public async Task<UserResponse> UserCreationOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var createRequest = context.GetInput<CreateUserRequest>()!;

        try
        {
            // Validate user data
            await context.CallActivityAsync("ValidateUserActivity", createRequest);

            // Create user in database
            var user = await context.CallActivityAsync<UserResponse>("CreateUserActivity", createRequest);

            // Send welcome notification
            await context.CallActivityAsync("SendWelcomeNotificationActivity", user);

            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            throw;
        }
    }

    [Function("UserUpdateOrchestrator")]
    public async Task<UserResponse?> UserUpdateOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var input = context.GetInput<dynamic>()!;
        var userId = Guid.Parse(input.UserId.ToString());
        var updateRequest = JsonConvert.DeserializeObject<UpdateUserRequest>(input.UpdateRequest.ToString());

        try
        {
            // Validate update data
            await context.CallActivityAsync("ValidateUserUpdateActivity", updateRequest);

            // Update user in database
            var user = await context.CallActivityAsync<UserResponse?>("UpdateUserActivity",
                new { UserId = userId, UpdateRequest = updateRequest });

            if (user != null)
                // Send update notification
                await context.CallActivityAsync("SendUserUpdateNotificationActivity", user);

            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            throw;
        }
    }

    [Function("DocumentCreationOrchestrator")]
    public async Task<DocumentResponse> DocumentCreationOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var createRequest = context.GetInput<CreateDocumentRequest>()!;

        try
        {
            // Validate document data
            await context.CallActivityAsync("ValidateDocumentActivity", createRequest);

            // Verify user exists
            await context.CallActivityAsync("VerifyUserExistsActivity", createRequest.UserId);

            // Create document in database
            var document = await context.CallActivityAsync<DocumentResponse>("CreateDocumentActivity", createRequest);

            // Initialize document content if it's a template
            if (createRequest.IsTemplate) await context.CallActivityAsync("InitializeTemplateActivity", document.Id);

            return document;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in document creation orchestration");
            throw;
        }
    }

    [Function("DocumentUpdateOrchestrator")]
    public async Task<DocumentResponse?> DocumentUpdateOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var input = context.GetInput<dynamic>()!;
        var documentId = Guid.Parse(input.DocumentId.ToString());
        var updateRequest = JsonConvert.DeserializeObject<UpdateDocumentRequest>(input.UpdateRequest.ToString());

        try
        {
            // Validate update data
            await context.CallActivityAsync("ValidateDocumentUpdateActivity", updateRequest);

            // Update document in database
            var document = await context.CallActivityAsync<DocumentResponse?>("UpdateDocumentActivity",
                new { DocumentId = documentId, UpdateRequest = updateRequest });

            if (document != null && updateRequest.Status.HasValue)
                // Handle status change notifications
                await context.CallActivityAsync("HandleDocumentStatusChangeActivity",
                    new { Document = document, NewStatus = updateRequest.Status.Value });

            return document;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
            throw;
        }
    }

    [Function("DocumentGenerationOrchestrator")]
    public async Task<DocumentResponse?> DocumentGenerationOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var generationRequest = context.GetInput<DocumentGenerationRequest>()!;

        try
        {
            // Get document details
            var document =
                await context.CallActivityAsync<DocumentResponse?>("GetDocumentActivity", generationRequest.DocumentId);
            if (document == null)
                throw new ArgumentException($"Document {generationRequest.DocumentId} not found");

            // Update status to InProgress
            await context.CallActivityAsync("UpdateDocumentStatusActivity",
                new { generationRequest.DocumentId, Status = DocumentStatus.InProgress });

            // Call AI Agent to generate document content
            var generatedContent = await context.CallActivityAsync<string>("GenerateDocumentContentActivity",
                new { Document = document, generationRequest.Parameters });

            // Save generated file
            var filePath = await context.CallActivityAsync<string>("SaveGeneratedDocumentActivity",
                new { generationRequest.DocumentId, Content = generatedContent });

            // Update document with file information and status
            var updatedDocument = await context.CallActivityAsync<DocumentResponse?>("FinalizeDocumentActivity",
                new { generationRequest.DocumentId, FilePath = filePath, Status = DocumentStatus.Generated });

            // Send completion notification
            if (updatedDocument != null)
                await context.CallActivityAsync("SendDocumentGenerationNotificationActivity", updatedDocument);

            return updatedDocument;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in document generation orchestration for document {DocumentId}",
                generationRequest.DocumentId);

            // Update status to failed on error
            await context.CallActivityAsync("UpdateDocumentStatusActivity",
                new { generationRequest.DocumentId, Status = DocumentStatus.Draft });

            throw;
        }
    }

    [Function("BulkDocumentProcessingOrchestrator")]
    public async Task<List<DocumentResponse>> BulkDocumentProcessingOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var documentIds = context.GetInput<List<Guid>>()!;
        var results = new List<DocumentResponse>();

        try
        {
            // Process documents in parallel with a maximum degree of parallelism
            const int maxParallelism = 5;

            for (var i = 0; i < documentIds.Count; i += maxParallelism)
            {
                var batch = documentIds.Skip(i).Take(maxParallelism);
                var batchTasks = batch.Select(id =>
                    context.CallActivityAsync<DocumentResponse>("ProcessSingleDocumentActivity", id));

                var batchResults = await Task.WhenAll(batchTasks);
                results.AddRange(batchResults);
            }

            return results;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in bulk document processing orchestration");
            throw;
        }
    }
}