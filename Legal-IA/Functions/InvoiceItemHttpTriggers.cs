using FluentValidation;
using Legal_IA.DTOs;
using Legal_IA.Enums;
using Legal_IA.Models;
using Legal_IA.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions;

public class InvoiceItemHttpTriggers
{
    [Function("GetInvoiceItemsByCurrentUser")]
    public async Task<IActionResult> GetInvoiceItemsByCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "invoice-items/user")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User)))
            return new UnauthorizedResult();
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out _))
            return new BadRequestObjectResult("Invalid or missing UserId in JWT");
        var instanceId =
            await client.ScheduleNewOrchestrationInstanceAsync("InvoiceItemGetByUserIdOrchestrator", jwtResult.UserId);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var items = response.ReadOutputAs<List<InvoiceItem>>();
            if (items == null || items.Count == 0) return new NotFoundResult();
            return new OkObjectResult(items);
        }

        return new StatusCodeResult(500);
    }

    [Function("CreateInvoiceItemByCurrentUser")]
    public async Task<IActionResult> CreateInvoiceItemByCurrentUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "invoice-items/user")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User))) return new UnauthorizedResult();
        var items = await req.ReadFromJsonAsync<List<InvoiceItem>>();
        if (items == null || items.Count == 0) return new BadRequestResult();
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out var userId))
            return new BadRequestObjectResult("Invalid or missing UserId in JWT");
        // Optionally, link items to invoice owned by user (add validation here if needed)
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceItemCreateOrchestrator", items);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var created = response.ReadOutputAs<List<InvoiceItem>>();
            if (created == null) return new StatusCodeResult(500);
            return new OkObjectResult(created);
        }
        return new StatusCodeResult(500);
    }

    /// <summary>
    /// Batch update of invoice items for the current user. Returns updated items and failed IDs if any.
    /// </summary>
    [Function("UpdateInvoiceItemByUser")]
    public async Task<IActionResult> UpdateInvoiceItemByUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "invoice-items/user")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        var logger = context.GetLogger("UpdateInvoiceItemByUser");
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User))) return new UnauthorizedResult();
        var dtos = await req.ReadFromJsonAsync<List<BatchUpdateInvoiceItemRequest>>();
        if (dtos == null || dtos.Count == 0)
        {
            logger.LogWarning("Batch update request is empty");
            return ProblemDetailsHelper.ValidationProblem(new[] { "Batch update request is empty" });
        }
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out _))
        {
            logger.LogWarning("Invalid or missing UserId in JWT");
            return ProblemDetailsHelper.ValidationProblem(new[] { "Invalid or missing UserId in JWT" });
        }
        // Validate all items using FluentValidation
        var validator = context.InstanceServices.GetService(typeof(IValidator<BatchUpdateInvoiceItemRequest>)) as IValidator<BatchUpdateInvoiceItemRequest>;
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
        logger.LogInformation($"User {jwtResult.UserId} batch updating {dtos.Count} invoice items");
        var input = new BatchUpdateInvoiceItemOrchestratorInput
        {
            UserId = new Guid(jwtResult.UserId),
            UpdateRequests = dtos
        };
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("PatchInvoiceItemOrchestrator", input);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var updated = response.ReadOutputAs<List<InvoiceItem>>();
            var updatedIds = updated?.Select(x => x.Id).ToHashSet() ?? new HashSet<Guid>();
            var failedIds = dtos.Select(x => x.ItemId).Where(id => !updatedIds.Contains(id)).ToList();
            if (updated == null || updated.Count == 0)
            {
                logger.LogWarning("No invoice items were updated");
                return new NotFoundObjectResult(new { message = "No invoice items were updated", failedIds });
            }
            if (failedIds.Count > 0)
            {
                logger.LogWarning($"Some invoice items failed to update: {string.Join(", ", failedIds)}");
                return new OkObjectResult(new { updated, failedIds });
            }
            return new OkObjectResult(updated);
        }
        logger.LogError("Batch update orchestration failed");
        return new StatusCodeResult(500);
    }

    /// <summary>
    /// Delete an invoice item for the current user by ID.
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
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User))) return new UnauthorizedResult();
        if (!Guid.TryParse(id, out var itemId))
        {
            logger.LogWarning($"Invalid itemId: {id}");
            return new BadRequestObjectResult($"Invalid itemId: {id}");
        }
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out var userId))
        {
            logger.LogWarning("Invalid or missing UserId in JWT");
            return new BadRequestObjectResult("Invalid or missing UserId in JWT");
        }
        var input = new DeleteInvoiceItemOrchestratorInput
        {
            ItemId = itemId,
            UserId = userId
        };
        logger.LogInformation($"User {userId} attempting to delete invoice item {itemId}");
        var instanceId = await client.ScheduleNewOrchestrationInstanceAsync("InvoiceItemDeleteOrchestrator", input);
        var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var deleted = response.ReadOutputAs<bool>();
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
}