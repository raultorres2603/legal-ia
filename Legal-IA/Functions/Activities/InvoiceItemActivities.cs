using Legal_IA.Interfaces.Repositories;
using Legal_IA.Interfaces.Services;
using Legal_IA.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Activities;

public class InvoiceItemActivities(IInvoiceItemRepository invoiceItemRepository, ICacheService cacheService)
{
    [Function(nameof(InvoiceItemGetAllActivity))]
    public async Task<List<InvoiceItem>> InvoiceItemGetAllActivity([ActivityTrigger] object input, FunctionContext context)
    {
        var log = context.GetLogger("InvoiceItemGetAllActivity");
        log.LogInformation("[InvoiceItemGetAllActivity] Activity started");
        const string cacheKey = "invoiceitems:all";
        var cached = await cacheService.GetAsync<List<InvoiceItem>>(cacheKey);
        if (cached != null)
        {
            log.LogInformation("[InvoiceItemGetAllActivity] Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        var items = (await invoiceItemRepository.GetAllAsync()).ToList();
        await cacheService.SetAsync(cacheKey, items);
        log.LogInformation($"[InvoiceItemGetAllActivity] Activity completed with {items.Count} items");
        return items;
    }

    [Function(nameof(InvoiceItemGetByIdActivity))]
    public async Task<InvoiceItem?> InvoiceItemGetByIdActivity([ActivityTrigger] Guid id, FunctionContext context)
    {
        var log = context.GetLogger("InvoiceItemGetByIdActivity");
        log.LogInformation($"[InvoiceItemGetByIdActivity] Activity started for id {id}");
        var cacheKey = $"invoiceitems:{id}";
        var cached = await cacheService.GetAsync<InvoiceItem>(cacheKey);
        if (cached != null)
        {
            log.LogInformation("[InvoiceItemGetByIdActivity] Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        var item = await invoiceItemRepository.GetByIdAsync(id);
        if (item != null) await cacheService.SetAsync(cacheKey, item);
        log.LogInformation($"[InvoiceItemGetByIdActivity] Activity completed for id {id}");
        return item;
    }

    [Function(nameof(InvoiceItemCreateActivity))]
    public async Task<InvoiceItem> InvoiceItemCreateActivity([ActivityTrigger] InvoiceItem item, FunctionContext context)
    {
        var log = context.GetLogger("InvoiceItemCreateActivity");
        log.LogInformation("[InvoiceItemCreateActivity] Activity started");
        var created = await invoiceItemRepository.AddAsync(item);
        await cacheService.RemoveByPatternAsync("invoiceitems");
        log.LogInformation($"[InvoiceItemCreateActivity] Activity completed, created item: {created.Id}");
        return created;
    }

    [Function(nameof(InvoiceItemUpdateActivity))]
    public async Task<InvoiceItem> InvoiceItemUpdateActivity([ActivityTrigger] InvoiceItem item, FunctionContext context)
    {
        var log = context.GetLogger("InvoiceItemUpdateActivity");
        log.LogInformation("[InvoiceItemUpdateActivity] Activity started");
        var updated = await invoiceItemRepository.UpdateAsync(item);
        await cacheService.RemoveByPatternAsync("invoiceitems");
        log.LogInformation($"[InvoiceItemUpdateActivity] Activity completed, updated item: {updated.Id}");
        return updated;
    }

    [Function(nameof(InvoiceItemDeleteActivity))]
    public async Task<bool> InvoiceItemDeleteActivity([ActivityTrigger] Guid id, FunctionContext context)
    {
        var log = context.GetLogger("InvoiceItemDeleteActivity");
        log.LogInformation($"[InvoiceItemDeleteActivity] Activity started for id {id}");
        var deleted = await invoiceItemRepository.DeleteAsync(id);
        await cacheService.RemoveByPatternAsync("invoiceitems");
        log.LogInformation($"[InvoiceItemDeleteActivity] Activity completed for id {id}, deleted: {deleted}");
        return deleted;
    }

    [Function(nameof(InvoiceItemGetByUserIdActivity))]
    public async Task<List<InvoiceItem>> InvoiceItemGetByUserIdActivity([ActivityTrigger] Guid userId)
    {
        var cacheKey = $"invoiceitems:user:{userId}";
        var cached = await cacheService.GetAsync<List<InvoiceItem>>(cacheKey);
        if (cached != null) return cached;
        var items = (await invoiceItemRepository.GetByUserIdAsync(userId)).ToList();
        await cacheService.SetAsync(cacheKey, items);
        return items;
    }
}
