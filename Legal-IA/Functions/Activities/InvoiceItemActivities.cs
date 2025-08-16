using System.Text.Json;
using Legal_IA.DTOs;
using Legal_IA.Shared.Enums;
using Legal_IA.Interfaces.Services;
using Legal_IA.Shared.Models;
using Legal_IA.Shared.Repositories.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Activities;

/// <summary>
///     Activity functions for invoice item operations, including caching and repository access.
/// </summary>
public class InvoiceItemActivities(
    IInvoiceItemRepository invoiceItemRepository,
    IInvoiceRepository invoiceRepository,
    ICacheService cacheService)
{
    /// <summary>
    ///     Gets all invoice items, using cache if available.
    /// </summary>
    [Function(nameof(InvoiceItemGetAllActivity))]
    public async Task<List<InvoiceItem>> InvoiceItemGetAllActivity([ActivityTrigger] object input,
        FunctionContext context)
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

    /// <summary>
    ///     Gets an invoice item by its ID, using cache if available.
    /// </summary>
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

    /// <summary>
    ///     Creates a new invoice item and invalidates related cache.
    /// </summary>
    [Function(nameof(InvoiceItemCreateActivity))]
    public async Task<InvoiceItem> InvoiceItemCreateActivity([ActivityTrigger] InvoiceItem item,
        FunctionContext context)
    {
        var log = context.GetLogger("InvoiceItemCreateActivity");
        log.LogInformation("[InvoiceItemCreateActivity] Activity started");
        var created = await invoiceItemRepository.AddAsync(item);
        await InvalidateUserInvoiceItemCache(item.InvoiceId);
        log.LogInformation($"[InvoiceItemCreateActivity] Activity completed, created item: {created.Id}");
        return created;
    }

    /// <summary>
    ///     Deletes an invoice item and invalidates related cache.
    /// </summary>
    [Function(nameof(InvoiceItemDeleteActivity))]
    public async Task<bool> InvoiceItemDeleteActivity([ActivityTrigger] Guid id, FunctionContext context)
    {
        var log = context.GetLogger("InvoiceItemDeleteActivity");
        log.LogInformation($"[InvoiceItemDeleteActivity] Activity started for id {id}");
        var item = await invoiceItemRepository.GetByIdAsync(id);
        var deleted = await invoiceItemRepository.DeleteAsync(id);
        if (item != null) await InvalidateUserInvoiceItemCache(item.InvoiceId);
        log.LogInformation($"[InvoiceItemDeleteActivity] Activity completed for id {id}, deleted: {deleted}");
        return deleted;
    }

    /// <summary>
    ///     Gets all invoice items for a specific user, using cache if available.
    /// </summary>
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

    /// <summary>
    ///     Validates ownership and pending status of an invoice item.
    /// </summary>
    [Function("InvoiceItemValidateOwnershipAndPendingActivity")]
    public async Task<bool> InvoiceItemValidateOwnershipAndPendingActivity([ActivityTrigger] object input,
        FunctionContext context)
    {
        var log = context.GetLogger("InvoiceItemValidateOwnershipAndPendingActivity");
        var inputElement = (JsonElement)input;
        var itemId = inputElement.GetProperty("ItemId").GetGuid();
        var userId = inputElement.GetProperty("UserId").GetGuid();
        log.LogInformation(
            $"[InvoiceItemValidateOwnershipAndPendingActivity] Validating item {itemId} for user {userId}");
        var item = await invoiceItemRepository.GetByIdAsync(itemId);
        if (item == null)
        {
            log.LogWarning($"[InvoiceItemValidateOwnershipAndPendingActivity] Item {itemId} not found");
            return false;
        }

        var invoice = await invoiceRepository.GetByIdAsync(item.InvoiceId);
        if (invoice == null)
        {
            log.LogWarning($"[InvoiceItemValidateOwnershipAndPendingActivity] Invoice {item.InvoiceId} not found");
            return false;
        }

        if (invoice.UserId != userId)
        {
            log.LogWarning(
                $"[InvoiceItemValidateOwnershipAndPendingActivity] User {userId} does not own invoice {invoice.Id}");
            return false;
        }

        if (invoice.Status != InvoiceStatus.Pending)
        {
            log.LogWarning($"[InvoiceItemValidateOwnershipAndPendingActivity] Invoice {invoice.Id} is not pending");
            return false;
        }

        log.LogInformation($"[InvoiceItemValidateOwnershipAndPendingActivity] Validation passed for item {itemId}");
        return true;
    }

    [Function("PatchInvoiceItemActivity")]
    public async Task<InvoiceItem?> PatchInvoiceItemActivity([ActivityTrigger] object input, FunctionContext context)
    {
        var log = context.GetLogger("PatchInvoiceItemActivity");
        var inputElement = (JsonElement)input;
        var itemId = inputElement.GetProperty("ItemId").GetGuid();
        var updateRequest =
            JsonSerializer.Deserialize<UpdateInvoiceItemRequest>(inputElement.GetProperty("UpdateRequest")
                .GetRawText());
        var existing = await invoiceItemRepository.GetByIdAsync(itemId);
        if (existing == null)
        {
            log.LogWarning($"[PatchInvoiceItemActivity] InvoiceItem {itemId} not found");
            return null;
        }

        PatchInvoiceItem(existing, updateRequest);
        var updated = await invoiceItemRepository.UpdateAsync(existing);
        await InvalidateUserInvoiceItemCache(existing.InvoiceId);
        log.LogInformation($"[PatchInvoiceItemActivity] Patched item: {updated.Id}");
        return updated;
    }

    private void PatchInvoiceItem(InvoiceItem existing, UpdateInvoiceItemRequest updateRequest)
    {
        if (updateRequest.Description != null) existing.Description = updateRequest.Description;
        if (updateRequest.Quantity != null) existing.Quantity = updateRequest.Quantity.Value;
        if (updateRequest.UnitPrice != null) existing.UnitPrice = updateRequest.UnitPrice.Value;
        if (updateRequest.VAT != null) existing.VAT = updateRequest.VAT.Value;
        if (updateRequest.IRPF != null) existing.IRPF = updateRequest.IRPF.Value;
        if (updateRequest.Total != null) existing.Total = updateRequest.Total.Value;
    }

    private async Task InvalidateUserInvoiceItemCache(Guid invoiceId)
    {
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId);
        if (invoice != null)
        {
            await cacheService.RemoveByPatternAsync($"invoiceitems:user:{invoice.UserId}");
            await cacheService.RemoveAsync($"invoices:user:{invoice.UserId}");
            await cacheService.RemoveByPatternAsync($"invoices:user:{invoice.UserId}");
        }
    }
}