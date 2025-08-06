using Legal_IA.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace Legal_IA.Functions.Orchestrators;

public static class InvoiceItemOrchestrators
{
    [Function(nameof(InvoiceItemGetAllOrchestrator))]
    public static async Task<List<InvoiceItem>> InvoiceItemGetAllOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        return await context.CallActivityAsync<List<InvoiceItem>>("InvoiceItemGetAllActivity", null);
    }

    [Function(nameof(InvoiceItemGetByIdOrchestrator))]
    public static async Task<InvoiceItem?> InvoiceItemGetByIdOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var id = context.GetInput<Guid>();
        return await context.CallActivityAsync<InvoiceItem?>("InvoiceItemGetByIdActivity", id);
    }

    [Function(nameof(InvoiceItemCreateOrchestrator))]
    public static async Task<InvoiceItem> InvoiceItemCreateOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var item = context.GetInput<InvoiceItem>();
        return await context.CallActivityAsync<InvoiceItem>("InvoiceItemCreateActivity", item);
    }

    [Function(nameof(InvoiceItemUpdateOrchestrator))]
    public static async Task<InvoiceItem> InvoiceItemUpdateOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var item = context.GetInput<InvoiceItem>();
        return await context.CallActivityAsync<InvoiceItem>("InvoiceItemUpdateActivity", item);
    }

    [Function(nameof(InvoiceItemDeleteOrchestrator))]
    public static async Task<bool> InvoiceItemDeleteOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var id = context.GetInput<Guid>();
        return await context.CallActivityAsync<bool>("InvoiceItemDeleteActivity", id);
    }

    [Function("InvoiceItemGetByUserIdOrchestrator")]
    public static async Task<List<InvoiceItem>> InvoiceItemGetByUserIdOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var userId = context.GetInput<Guid>();
        return await context.CallActivityAsync<List<InvoiceItem>>("InvoiceItemGetByUserIdActivity", userId);
    }
}