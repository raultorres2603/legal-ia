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
        const string cacheKey = "invoiceitems:all";
        var cached = await cacheService.GetAsync<List<InvoiceItem>>(cacheKey);
        if (cached != null)
        {
            log.LogInformation("[InvoiceItemGetAllActivity] Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        var items = (await invoiceItemRepository.GetAllAsync()).ToList();
        await cacheService.SetAsync(cacheKey, items);
        return items;
    }

    [Function(nameof(InvoiceItemGetByIdActivity))]
    public async Task<InvoiceItem?> InvoiceItemGetByIdActivity([ActivityTrigger] Guid id, FunctionContext context)
    {
        var log = context.GetLogger("InvoiceItemGetByIdActivity");
        var cacheKey = $"invoiceitems:{id}";
        var cached = await cacheService.GetAsync<InvoiceItem>(cacheKey);
        if (cached != null)
        {
            log.LogInformation("[InvoiceItemGetByIdActivity] Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }
        var item = await invoiceItemRepository.GetByIdAsync(id);
        if (item != null) await cacheService.SetAsync(cacheKey, item);
        return item;
    }

    [Function(nameof(InvoiceItemCreateActivity))]
    public async Task<InvoiceItem> InvoiceItemCreateActivity([ActivityTrigger] InvoiceItem item)
    {
        var created = await invoiceItemRepository.AddAsync(item);
        await cacheService.RemoveByPatternAsync("invoiceitems");
        return created;
    }

    [Function(nameof(InvoiceItemUpdateActivity))]
    public async Task<InvoiceItem> InvoiceItemUpdateActivity([ActivityTrigger] InvoiceItem item)
    {
        var updated = await invoiceItemRepository.UpdateAsync(item);
        await cacheService.RemoveByPatternAsync("invoiceitems");
        return updated;
    }

    [Function(nameof(InvoiceItemDeleteActivity))]
    public async Task<bool> InvoiceItemDeleteActivity([ActivityTrigger] Guid id)
    {
        var deleted = await invoiceItemRepository.DeleteAsync(id);
        await cacheService.RemoveByPatternAsync("invoiceitems");
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
