using System.Text.Json;
using Legal_IA.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Orchestrators;

public static class InvoiceOrchestrators
{
    [Function(nameof(InvoiceGetAllOrchestrator))]
    public static async Task<List<Invoice>> InvoiceGetAllOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("InvoiceGetAllOrchestrator");
        logger.LogInformation("[InvoiceGetAllOrchestrator] Orchestrator started");
        var invoices = await context.CallActivityAsync<List<Invoice>>("InvoiceGetAllActivity", null!);
        logger.LogInformation($"[InvoiceGetAllOrchestrator] Orchestrator completed with {invoices?.Count ?? 0} invoices");
        return invoices ?? throw new InvalidOperationException();
    }

    [Function(nameof(InvoiceGetByIdOrchestrator))]
    public static async Task<Invoice?> InvoiceGetByIdOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("InvoiceGetByIdOrchestrator");
        var id = context.GetInput<Guid>();
        logger.LogInformation($"[InvoiceGetByIdOrchestrator] Orchestrator started for id {id}");
        var invoice = await context.CallActivityAsync<Invoice?>("InvoiceGetByIdActivity", id);
        if (invoice == null)
            logger.LogInformation($"[InvoiceGetByIdOrchestrator] No invoice found for id {id}");
        else
            logger.LogInformation($"[InvoiceGetByIdOrchestrator] Orchestrator completed for id {id}");
        return invoice;
    }

    [Function(nameof(InvoiceCreateOrchestrator))]
    public static async Task<Invoice> InvoiceCreateOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("InvoiceCreateOrchestrator");
        var invoice = context.GetInput<Invoice>();
        if (invoice != null)
        {
            logger.LogInformation("Orchestrator started: InvoiceCreateOrchestrator for user {UserId}", invoice.UserId);
            var created = await context.CallActivityAsync<Invoice>("InvoiceCreateActivity", invoice);
            logger.LogInformation("Orchestrator completed: InvoiceCreateOrchestrator for invoice {InvoiceId}",
                created.Id);
            return created;
        }
        logger.LogError("Orchestrator failed: InvoiceCreateOrchestrator received null invoice input");
        throw new ArgumentNullException(nameof(invoice), "Invoice input cannot be null");
    }

    [Function(nameof(InvoiceUpdateOrchestrator))]
    public static async Task<Invoice> InvoiceUpdateOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("InvoiceUpdateOrchestrator");
        var invoice = context.GetInput<Invoice>();
        if (invoice != null)
        {
            logger.LogInformation("Orchestrator started: InvoiceUpdateOrchestrator for invoice {InvoiceId}",
                invoice.Id);
            var updated = await context.CallActivityAsync<Invoice>("InvoiceUpdateActivity", invoice);
            logger.LogInformation("Orchestrator completed: InvoiceUpdateOrchestrator for invoice {InvoiceId}",
                updated.Id);
            return updated;
        }
        logger.LogError("Orchestrator failed: InvoiceUpdateOrchestrator received null invoice input");
        throw new ArgumentNullException(nameof(invoice), "Invoice input cannot be null");
    }

    [Function(nameof(InvoiceDeleteOrchestrator))]
    public static async Task<bool> InvoiceDeleteOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("InvoiceDeleteOrchestrator");
        var id = context.GetInput<Guid>();
        logger.LogInformation("Orchestrator started: InvoiceDeleteOrchestrator for id {Id}", id);
        var deleted = await context.CallActivityAsync<bool>("InvoiceDeleteActivity", id);
        logger.LogInformation("Orchestrator completed: InvoiceDeleteOrchestrator for id {Id} - Deleted: {Deleted}", id, deleted);
        return deleted;
    }

    [Function("InvoiceGetByUserIdOrchestrator")]
    public static async Task<List<Invoice>> InvoiceGetByUserIdOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("InvoiceGetByUserIdOrchestrator");
        var userId = context.GetInput<Guid>();
        logger.LogInformation("Orchestrator started for userId: {UserId}", userId);
        var invoices = await context.CallActivityAsync<List<Invoice>>("InvoiceGetByUserIdActivity", userId);
        if (invoices == null)
        {
            logger.LogWarning("No invoices returned for userId: {UserId}", userId);
        }
        else
        {
            logger.LogInformation("Orchestrator completed for userId: {UserId} with {Count} invoices", userId, invoices.Count);
        }
        return invoices!;
    }

    [Function("InvoiceUpdateByCurrentUserOrchestrator")]
    public static async Task<Invoice> InvoiceUpdateByCurrentUserOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("InvoiceUpdateByCurrentUserOrchestrator");
        var input = context.GetInput<dynamic>();
            // Fix: Properly handle System.Text.Json.JsonElement
            var inputElement = (JsonElement)(input ?? throw new InvalidOperationException());
            var invoiceElement = inputElement.GetProperty("Invoice");
            var invoice = JsonSerializer.Deserialize<Invoice>(invoiceElement.GetRawText());
            var userId = inputElement.GetProperty("UserId").GetGuid();
            
                logger.LogInformation(
                    $"Orchestrator started: InvoiceUpdateByCurrentUserOrchestrator for invoice {invoice!.Id} and user {userId}");
                var activityInput = new { Invoice = invoice, UserId = userId };
                var updated =
                    await context.CallActivityAsync<Invoice>("UpdateInvoiceByCurrentUserActivity", activityInput);
                logger.LogInformation(
                    "Orchestrator completed: InvoiceUpdateByCurrentUserOrchestrator for invoice {InvoiceId}",
                    updated.Id);
                return updated;
        
    }

    [Function("InvoiceDeleteByCurrentUserOrchestrator")]
    public static async Task<bool> InvoiceDeleteByCurrentUserOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("InvoiceDeleteByCurrentUserOrchestrator");
        var input = context.GetInput<dynamic>();
        if (input != null)
        {
            var invoiceId = (Guid)input.InvoiceId;
            var userId = (Guid)input.UserId;
            logger.LogInformation($"Orchestrator started: InvoiceDeleteByCurrentUserOrchestrator for invoice {invoiceId} and user {userId}");
            var activityInput = new { InvoiceId = invoiceId, UserId = userId };
            var invoice = await context.CallActivityAsync<Invoice>("InvoiceGetByIdAndUserIdActivity", activityInput);
            if (invoice.Status == Enums.InvoiceStatus.Pending)
            {
                var deleted = await context.CallActivityAsync<bool>("InvoiceDeleteActivity", invoiceId);
                logger.LogInformation($"Invoice {invoiceId} deleted by user {userId}: {deleted}");
                return deleted;
            }
            logger.LogWarning($"Invoice {invoiceId} not deleted. Either not found, not owned by user, or not pending.");
        }
        logger.LogError("Orchestrator failed: InvoiceDeleteByCurrentUserOrchestrator received null input");
        return false;
    }
}