using System.Text.Json;
using Legal_IA.DTOs;
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
    public static async Task<List<InvoiceItem>> InvoiceItemCreateOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("InvoiceItemCreateOrchestrator");
        var items = context.GetInput<List<InvoiceItem>>();
        logger.LogInformation($"[InvoiceItemCreateOrchestrator] Orchestrator started for {items?.Count ?? 0} items");
        var results = new List<InvoiceItem>();
        if (items != null)
        {
            foreach (var item in items)
            {
                results.Add(await context.CallActivityAsync<InvoiceItem>("InvoiceItemCreateActivity", item));
            }
        }
        logger.LogInformation($"[InvoiceItemCreateOrchestrator] Orchestrator completed, created {results.Count} items");
        return results;
    }


    [Function("InvoiceItemDeleteOrchestrator")]
    public static async Task<bool> InvoiceItemDeleteOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("InvoiceItemDeleteOrchestrator");
        var input = context.GetInput<DeleteInvoiceItemOrchestratorInput>();
        if (input == null)
        {
            logger.LogError("InvoiceItemDeleteOrchestrator received null input");
            return false;
        }
        var itemId = input.ItemId;
        var userId = input.UserId;
        logger.LogInformation($"[InvoiceItemDeleteOrchestrator] Started for item {itemId} by user {userId}");
        var isValid = await context.CallActivityAsync<bool>("InvoiceItemValidateOwnershipAndPendingActivity",
            new { ItemId = itemId, UserId = userId });
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
        logger.LogInformation(
            $"[InvoiceItemGetByUserIdOrchestrator] Orchestrator completed for userId {userId}, returned {result.Count} items");
        return result;
    }

    [Function("PatchInvoiceItemOrchestrator")]
    public static async Task<List<InvoiceItem>> PatchInvoiceItemOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("PatchInvoiceItemOrchestrator");
        var input = context.GetInput<BatchUpdateInvoiceItemOrchestratorInput>();
        if (input == null || input.UpdateRequests == null)
        {
            logger.LogError("PatchInvoiceItemOrchestrator received null or invalid input");
            return new List<InvoiceItem>();
        }
        var results = new List<InvoiceItem>();
        foreach (var req in input.UpdateRequests)
        {
            var inputObj = new
            {
                req.ItemId,
                input.UserId,
                req.UpdateRequest
            };
            var updated = await context.CallActivityAsync<InvoiceItem?>("PatchInvoiceItemActivity", inputObj);
            if (updated != null) results.Add(updated);
        }
        logger.LogInformation($"[PatchInvoiceItemOrchestrator] Orchestrator completed, updated {results.Count} items");
        return results;
    }
}