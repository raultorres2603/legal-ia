using System.Text.Json;
using Legal_IA.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Orchestrators;

public static class InvoiceItemOrchestrators
{
    [Function(nameof(InvoiceItemGetAllOrchestrator))]
    public static async Task<List<InvoiceItem>> InvoiceItemGetAllOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("InvoiceItemGetAllOrchestrator");
        logger.LogInformation("[InvoiceItemGetAllOrchestrator] Orchestrator started");
        var result = await context.CallActivityAsync<List<InvoiceItem>>("InvoiceItemGetAllActivity", null);
        logger.LogInformation($"[InvoiceItemGetAllOrchestrator] Orchestrator completed, returned {result.Count} items");
        return result;
    }

    [Function(nameof(InvoiceItemGetByIdOrchestrator))]
    public static async Task<InvoiceItem?> InvoiceItemGetByIdOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("InvoiceItemGetByIdOrchestrator");
        var id = context.GetInput<Guid>();
        logger.LogInformation($"[InvoiceItemGetByIdOrchestrator] Orchestrator started for id {id}");
        var result = await context.CallActivityAsync<InvoiceItem?>("InvoiceItemGetByIdActivity", id);
        logger.LogInformation($"[InvoiceItemGetByIdOrchestrator] Orchestrator completed for id {id}");
        return result;
    }

    [Function(nameof(InvoiceItemCreateOrchestrator))]
    public static async Task<InvoiceItem> InvoiceItemCreateOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("InvoiceItemCreateOrchestrator");
        var item = context.GetInput<InvoiceItem>();
        logger.LogInformation("[InvoiceItemCreateOrchestrator] Orchestrator started");
        var result = await context.CallActivityAsync<InvoiceItem>("InvoiceItemCreateActivity", item);
        logger.LogInformation($"[InvoiceItemCreateOrchestrator] Orchestrator completed, created item: {result.Id}");
        return result;
    }

    [Function("InvoiceItemUpdateOrchestrator")]
    public static async Task<InvoiceItem?> InvoiceItemUpdateOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("InvoiceItemUpdateOrchestrator");
        var input = context.GetInput<dynamic>();
        Guid itemId = input.ItemId;
        Guid userId = input.UserId;
        var update = input.Update;
        logger.LogInformation($"[InvoiceItemUpdateOrchestrator] Started for item {itemId} by user {userId}");
        var isValid = await context.CallActivityAsync<bool>("InvoiceItemValidateOwnershipAndPendingActivity", new { ItemId = itemId, UserId = userId });
        if (!isValid)
        {
            logger.LogWarning($"[InvoiceItemUpdateOrchestrator] Validation failed for item {itemId} by user {userId}");
            return null;
        }
        var updated = await context.CallActivityAsync<InvoiceItem>("InvoiceItemUpdateActivity", update);
        logger.LogInformation($"[InvoiceItemUpdateOrchestrator] Updated item {itemId}");
        return updated;
    }

    [Function("InvoiceItemDeleteOrchestrator")]
    public static async Task<bool> InvoiceItemDeleteOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("InvoiceItemDeleteOrchestrator");
        var inputElement = (JsonElement)(context.GetInput<object>() ?? throw new InvalidOperationException());
        var itemId = inputElement.GetProperty("ItemId").GetGuid();
        var userId = inputElement.GetProperty("UserId").GetGuid();
        logger.LogInformation($"[InvoiceItemDeleteOrchestrator] Started for item {itemId} by user {userId}");
        var isValid = await context.CallActivityAsync<bool>("InvoiceItemValidateOwnershipAndPendingActivity", new { ItemId = itemId, UserId = userId });
        if (!isValid)
        {
            logger.LogWarning($"[InvoiceItemDeleteOrchestrator] Validation failed for item {itemId} by user {userId}");
            return false;
        }
        var deleted = await context.CallActivityAsync<bool>("InvoiceItemDeleteActivity", itemId);
        logger.LogInformation($"[InvoiceItemDeleteOrchestrator] Deleted item {itemId}: {deleted}");
        return deleted;
    }

    [Function("InvoiceItemGetByUserIdOrchestrator")]
    public static async Task<List<InvoiceItem>> InvoiceItemGetByUserIdOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("InvoiceItemGetByUserIdOrchestrator");
        var userId = context.GetInput<Guid>();
        logger.LogInformation($"[InvoiceItemGetByUserIdOrchestrator] Orchestrator started for userId {userId}");
        var result = await context.CallActivityAsync<List<InvoiceItem>>("InvoiceItemGetByUserIdActivity", userId);
        logger.LogInformation($"[InvoiceItemGetByUserIdOrchestrator] Orchestrator completed for userId {userId}, returned {result.Count} items");
        return result;
    }
}