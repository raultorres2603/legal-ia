using Legal_IA.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace Legal_IA.Functions.Orchestrators;

public static class InvoiceOrchestrators
{
    [Function(nameof(InvoiceGetAllOrchestrator))]
    public static async Task<List<Invoice>> InvoiceGetAllOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        return await context.CallActivityAsync<List<Invoice>>("InvoiceGetAllActivity", null);
    }

    [Function(nameof(InvoiceGetByIdOrchestrator))]
    public static async Task<Invoice?> InvoiceGetByIdOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var id = context.GetInput<Guid>();
        return await context.CallActivityAsync<Invoice?>("InvoiceGetByIdActivity", id);
    }

    [Function(nameof(InvoiceCreateOrchestrator))]
    public static async Task<Invoice> InvoiceCreateOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var invoice = context.GetInput<Invoice>();
        return await context.CallActivityAsync<Invoice>("InvoiceCreateActivity", invoice);
    }

    [Function(nameof(InvoiceUpdateOrchestrator))]
    public static async Task<Invoice> InvoiceUpdateOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var invoice = context.GetInput<Invoice>();
        return await context.CallActivityAsync<Invoice>("InvoiceUpdateActivity", invoice);
    }

    [Function(nameof(InvoiceDeleteOrchestrator))]
    public static async Task<bool> InvoiceDeleteOrchestrator([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var id = context.GetInput<Guid>();
        return await context.CallActivityAsync<bool>("InvoiceDeleteActivity", id);
    }

    [Function("InvoiceGetByUserIdOrchestrator")]
    public static async Task<List<Invoice>> InvoiceGetByUserIdOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var userId = context.GetInput<Guid>();
        return await context.CallActivityAsync<List<Invoice>>("InvoiceGetByUserIdActivity", userId);
    }
}