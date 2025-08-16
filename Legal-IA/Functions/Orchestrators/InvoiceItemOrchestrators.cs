using Legal_IA.DTOs;
using Legal_IA.Shared.Models;
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
        const int maxBatchSize = 50;
        const int maxConcurrency = 5;

        // Validate input
        if (items == null || items.Count == 0)
        {
            logger.LogError("InvoiceItemCreateOrchestrator received null or empty input");
            return new List<InvoiceItem>();
        }

        if (items.Count > maxBatchSize)
        {
            logger.LogError(
                $"InvoiceItemCreateOrchestrator batch size {items.Count} exceeds the maximum allowed ({maxBatchSize})");
            return new List<InvoiceItem>();
        }

        // Process creations concurrently in batches
        var results = await ProcessCreateBatchConcurrently(items, maxConcurrency, context);
        logger.LogInformation($"[InvoiceItemCreateOrchestrator] Orchestrator completed, created {results.Count} items");
        return results;
    }

    private static async Task<List<InvoiceItem>> ProcessCreateBatchConcurrently(
        List<InvoiceItem> items,
        int maxConcurrency,
        TaskOrchestrationContext context)
    {
        var results = new List<InvoiceItem>();
        var tasks = new List<Task<InvoiceItem>>();
        foreach (var item in items)
        {
            tasks.Add(context.CallActivityAsync<InvoiceItem>("InvoiceItemCreateActivity", item));
            if (tasks.Count == maxConcurrency)
            {
                var completed = await Task.WhenAll(tasks);
                results.AddRange(completed);
                tasks.Clear();
            }
        }

        if (tasks.Count > 0)
        {
            var completed = await Task.WhenAll(tasks);
            results.AddRange(completed);
        }

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

    [Function(nameof(PatchInvoiceItemOrchestrator))]
    public static async Task<BatchUpdateInvoiceItemResult> PatchInvoiceItemOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("PatchInvoiceItemOrchestrator");
        var input = context.GetInput<BatchUpdateInvoiceItemOrchestratorInput>();
        const int maxBatchSize = 50;
        const int maxConcurrency = 5;

        // Validate input
        var validationError = ValidateBatchInput(input, maxBatchSize);
        if (validationError != null)
        {
            logger.LogError($"PatchInvoiceItemOrchestrator error for user {input?.UserId}: {validationError}");
            return new BatchUpdateInvoiceItemResult
            {
                Success = false,
                Error = validationError
            };
        }

        // Process updates concurrently in batches
        var items = await ProcessBatchConcurrently(input, maxConcurrency, context);
        logger.LogInformation(
            $"[PatchInvoiceItemOrchestrator] Orchestrator completed, updated {items.Count} items for user {input.UserId}");
        return new BatchUpdateInvoiceItemResult
        {
            Success = true,
            Items = items
        };
    }

    private static string? ValidateBatchInput(BatchUpdateInvoiceItemOrchestratorInput? input, int maxBatchSize)
    {
        if (input == null)
            return "Input or update requests are null.";
        if (input.UpdateRequests.Count > maxBatchSize)
            return $"Batch size {input.UpdateRequests.Count} exceeds the maximum allowed ({maxBatchSize}).";
        return null;
    }

    private static async Task<List<InvoiceItem>> ProcessBatchConcurrently(
        BatchUpdateInvoiceItemOrchestratorInput input,
        int maxConcurrency,
        TaskOrchestrationContext context)
    {
        var results = new List<InvoiceItem>();
        var tasks = new List<Task<InvoiceItem?>>();
        foreach (var req in input.UpdateRequests)
        {
            var inputObj = new { req.ItemId, input.UserId, req.UpdateRequest };
            tasks.Add(context.CallActivityAsync<InvoiceItem?>("PatchInvoiceItemActivity", inputObj));
            if (tasks.Count == maxConcurrency)
            {
                var completed = await Task.WhenAll(tasks);
                results.AddRange(completed.Where(x => x != null)!);
                tasks.Clear();
            }
        }

        if (tasks.Count > 0)
        {
            var completed = await Task.WhenAll(tasks);
            results.AddRange(completed.Where(x => x != null)!);
        }

        return results;
    }
}