using FluentValidation;
using Legal_IA.DTOs;
using Legal_IA.Shared.Models;
using Legal_IA.Services;
using Legal_IA.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions;

/// <summary>
///     HTTP-triggered Azure Functions for invoice item operations by the current user.
/// </summary>
public class InvoiceItemHttpTriggers
{
    /// <summary>
    ///     Gets invoice items for the current user.
    /// </summary>
    [Function("GetInvoiceItemsByCurrentUser")]
    public async Task<IActionResult> GetInvoiceItemsByCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "invoice-items/user")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var (userId, errorResult) = await ValidateAndExtractUserId(req, client);
        if (errorResult != null) return errorResult;
        return await RunOrchestrationAndRespond(client, "InvoiceItemGetByUserIdOrchestrator", userId);
    }

    /// <summary>
    ///     Creates invoice items for the current user.
    /// </summary>
    [Function("CreateInvoiceItemByCurrentUser")]
    public async Task<IActionResult> CreateInvoiceItemByCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "invoice-items/user")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var (userId, errorResult) = await ValidateAndExtractUserId(req, client);
        if (errorResult != null) return errorResult;
        var items = await req.ReadFromJsonAsync<List<InvoiceItem>>();
        if (items == null || items.Count == 0) return new BadRequestResult();
        // Optionally, link items to invoice owned by user (add validation here if needed)
        return await RunOrchestrationAndRespond(client, "InvoiceItemCreateOrchestrator", items);
    }

    /// <summary>
    ///     Batch update of invoice items for the current user. Returns updated items and failed IDs if any.
    /// </summary>
    [Function("UpdateInvoiceItemByUser")]
    public async Task<IActionResult> UpdateInvoiceItemByUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "invoice-items/user")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var logger = context.GetLogger("UpdateInvoiceItemByUser");
        var (userId, errorResult) = await ValidateAndExtractUserId(req, client);
        if (errorResult != null) return errorResult;
        var dtos = await req.ReadFromJsonAsync<List<BatchUpdateInvoiceItemRequest>>();
        if (dtos == null || dtos.Count == 0)
        {
            logger.LogWarning("Batch update request is empty");
            return ProblemDetailsHelper.ValidationProblem(new[] { "Batch update request is empty" });
        }

        // Validate all items using FluentValidation
        var validator =
            context.InstanceServices.GetService(typeof(IValidator<BatchUpdateInvoiceItemRequest>)) as
                IValidator<BatchUpdateInvoiceItemRequest>;
        if (validator == null)
        {
            logger.LogError("BatchUpdateInvoiceItemRequestValidator is not registered in DI container.");
            return new StatusCodeResult(500);
        }

        var errors = new List<string>();
        foreach (var dto in dtos)
        {
            var result = await validator.ValidateAsync(dto);
            if (!result.IsValid)
                errors.AddRange(result.Errors.Select(e => $"ItemId {dto.ItemId}: {e.ErrorMessage}"));
        }

        if (errors.Count > 0)
        {
            logger.LogWarning($"Validation failed for batch update: {string.Join("; ", errors)}");
            return ProblemDetailsHelper.ValidationProblem(errors);
        }

        logger.LogInformation($"User {userId} batch updating {dtos.Count} invoice items");
        var input = new BatchUpdateInvoiceItemOrchestratorInput
        {
            UserId = userId,
            UpdateRequests = dtos
        };
        var response = await client.ScheduleNewOrchestrationInstanceAsync("PatchInvoiceItemOrchestrator", input);
        var orchestrationResult = await client.WaitForInstanceCompletionAsync(response, true, CancellationToken.None);
        if (orchestrationResult.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var result = orchestrationResult.ReadOutputAs<BatchUpdateInvoiceItemResult>();
            if (result == null)
            {
                logger.LogError("Orchestrator returned null result");
                return new StatusCodeResult(500);
            }

            if (!result.Success)
            {
                logger.LogWarning($"Batch update failed: {result.Error}");
                return ProblemDetailsHelper.ValidationProblem([result.Error ?? "Unknown error"]);
            }

            if (result.Items.Count == 0)
                return new NotFoundResult();
            return new OkObjectResult(result.Items);
        }

        return new StatusCodeResult(500);
    }

    /// <summary>
    ///     Delete an invoice item for the current user by ID.
    /// </summary>
    [Function("DeleteInvoiceItemByUser")]
    public async Task<IActionResult> DeleteInvoiceItemByUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "invoice-items/user/{id}")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client,
        string id)
    {
        var logger = context.GetLogger("DeleteInvoiceItemByUser");
        var (userId, errorResult) = await ValidateAndExtractUserId(req, client);
        if (errorResult != null) return errorResult;
        if (!Guid.TryParse(id, out var itemId))
        {
            logger.LogWarning($"Invalid itemId: {id}");
            return new BadRequestObjectResult($"Invalid itemId: {id}");
        }

        var input = new DeleteInvoiceItemOrchestratorInput
        {
            ItemId = itemId,
            UserId = userId
        };
        logger.LogInformation($"User {userId} attempting to delete invoice item {itemId}");
        var response = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceItemDeleteOrchestrator", input);
        var orchestrationResult = await client.WaitForInstanceCompletionAsync(response, true, CancellationToken.None);
        if (orchestrationResult.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var deleted = orchestrationResult.ReadOutputAs<bool>();
            if (!deleted)
            {
                logger.LogWarning($"Invoice item {itemId} not found or not deleted");
                return new NotFoundObjectResult(new { message = $"Invoice item {itemId} not found or not deleted" });
            }

            return new OkResult();
        }

        logger.LogError("Delete orchestration failed");
        return new StatusCodeResult(500);
    }

    // --- Private helpers ---

    /// <summary>
    ///     Validates JWT and extracts userId. Returns (userId, errorResult).
    /// </summary>
    private static async Task<(Guid userId, IActionResult errorResult)> ValidateAndExtractUserId(HttpRequestData req,
        DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User)))
            return (Guid.Empty, new UnauthorizedResult());
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out var userId))
            return (Guid.Empty, new BadRequestObjectResult("Invalid or missing UserId in JWT"));
        return (userId, null);
    }

    /// <summary>
    ///     Schedules an orchestration and returns the HTTP response.
    /// </summary>
    private static async Task<IActionResult> RunOrchestrationAndRespond(DurableTaskClient client,
        string orchestratorName, object input)
    {
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(orchestratorName, input);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            return new OkObjectResult(response.ReadOutputAs<object>());
        return new StatusCodeResult(500);
    }
}