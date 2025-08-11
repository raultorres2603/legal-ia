using System.Text.Json;
using Legal_IA.DTOs;
using Legal_IA.Enums;
using Legal_IA.Interfaces.Repositories;
using Legal_IA.Interfaces.Services;
using Legal_IA.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Activities;

public class InvoiceActivities(IInvoiceRepository invoiceRepository, ICacheService cacheService)
{
    [Function(nameof(InvoiceGetAllActivity))]
    public async Task<List<Invoice>> InvoiceGetAllActivity([ActivityTrigger] object input, FunctionContext context)
    {
        var log = context.GetLogger("InvoiceGetAllActivity");
        log.LogInformation("[InvoiceGetAllActivity] Activity started");
        const string cacheKey = "invoices:all";
        var cached = await cacheService.GetAsync<List<Invoice>>(cacheKey);
        if (cached != null)
        {
            log.LogInformation("[InvoiceGetAllActivity] Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }

        var invoices = (await invoiceRepository.GetAllAsync()).ToList();
        await cacheService.SetAsync(cacheKey, invoices);
        log.LogInformation($"[InvoiceGetAllActivity] Activity completed with {invoices.Count} invoices");
        return invoices;
    }

    [Function(nameof(InvoiceGetByIdActivity))]
    public async Task<Invoice?> InvoiceGetByIdActivity([ActivityTrigger] Guid id, FunctionContext context)
    {
        var log = context.GetLogger("InvoiceGetByIdActivity");
        log.LogInformation($"[InvoiceGetByIdActivity] Activity started for id {id}");
        var cacheKey = $"invoices:{id}";
        var cached = await cacheService.GetAsync<Invoice>(cacheKey);
        if (cached != null)
        {
            log.LogInformation("[InvoiceGetByIdActivity] Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }

        var invoice = await invoiceRepository.GetByIdAsync(id);
        if (invoice != null) await cacheService.SetAsync(cacheKey, invoice);
        log.LogInformation($"[InvoiceGetByIdActivity] Activity completed for id {id}");
        return invoice;
    }

    [Function("InvoiceGetByUserIdActivity")]
    public async Task<List<Invoice>> InvoiceGetByUserIdActivity([ActivityTrigger] Guid userId, FunctionContext context)
    {
        var log = context.GetLogger("InvoiceGetByUserIdActivity");
        var cacheKey = $"invoices:user:{userId}";
        var cached = await cacheService.GetAsync<List<Invoice>>(cacheKey);
        if (cached != null)
        {
            log.LogInformation("[InvoiceGetByUserIdActivity] Cache hit for key: {CacheKey}", cacheKey);
            return cached;
        }

        log.LogInformation("Fetching invoices for user {UserId} from repository", userId);
        var invoices = await invoiceRepository.GetInvoicesByUserIdAsync(userId);
        await cacheService.SetAsync(cacheKey, invoices);
        return invoices;
    }

    [Function(nameof(InvoiceCreateActivity))]
    public async Task<Invoice> InvoiceCreateActivity([ActivityTrigger] Invoice invoice, FunctionContext context)
    {
        var log = context.GetLogger("InvoiceCreateActivity");
        if (invoice.UserId == Guid.Empty)
            throw new ArgumentException("UserId must be set on Invoice");
        var created = await invoiceRepository.AddAsync(invoice);
        await cacheService.RemoveByPatternAsync("invoices");
        log.LogInformation("Invoice created and cache invalidated for pattern 'invoices'");
        return created;
    }


    [Function(nameof(InvoiceDeleteActivity))]
    public async Task<bool> InvoiceDeleteActivity([ActivityTrigger] Guid id, FunctionContext context)
    {
        var log = context.GetLogger("InvoiceDeleteActivity");
        await invoiceRepository.GetByIdAsync(id);
        var deleted = await invoiceRepository.DeleteAsync(id);
        await cacheService.RemoveByPatternAsync("invoices");
        log.LogInformation("Invoice deleted and related cache keys invalidated");
        return deleted;
    }

    // [Function(nameof(InvoiceUpdateActivity))]
    // public async Task<Invoice> InvoiceUpdateActivity([ActivityTrigger] Invoice invoice, FunctionContext context)
    // {
    //     var log = context.GetLogger("InvoiceUpdateActivity");
    //     var updated = await invoiceRepository.UpdateAsync(invoice);
    //     await cacheService.RemoveByPatternAsync("invoices");
    //     log.LogInformation("Invoice updated and cache invalidated for pattern 'invoices'");
    //     return updated;
    // }

    // [Function("UpdateInvoiceByCurrentUserActivity")]
    // public async Task<Invoice> UpdateInvoiceByCurrentUserActivity([ActivityTrigger] object input,
    //     FunctionContext context)
    // {
    //     var log = context.GetLogger("UpdateInvoiceByCurrentUserActivity");
    //     var inputElement = (JsonElement)input;
    //     var invoiceId = inputElement.GetProperty("InvoiceId").GetGuid();
    //     var updateRequest = JsonSerializer.Deserialize<UpdateInvoiceRequest>(inputElement.GetProperty("UpdateRequest").GetRawText());
    //     var userId = inputElement.GetProperty("UserId").GetGuid();
    //     var existing = await invoiceRepository.GetByIdAsync(invoiceId);
    //     if (existing == null || existing.UserId != userId)
    //         throw new UnauthorizedAccessException("Invoice not found or does not belong to user");
    //     if (existing.Status != InvoiceStatus.Pending)
    //         throw new InvalidOperationException("Only pending invoices can be updated");

    //    PatchInvoice(existing, updateRequest);

    //     var updated = await invoiceRepository.UpdateAsync(existing);
    //     // Invalidate user invoice list cache (exact key and pattern)
    //     await cacheService.RemoveAsync($"invoices:user:{userId}");
    //     await cacheService.RemoveByPatternAsync($"invoices:user:{userId}");
    //     await cacheService.RemoveByPatternAsync($"invoices:{existing.Id}");
    //     log.LogInformation("Invoice updated by current user and related cache keys invalidated");
    //     return updated;
    // }
    [Function("InvoiceGetByIdAndUserIdActivity")]
    public async Task<Invoice?> InvoiceGetByIdAndUserIdActivity([ActivityTrigger] dynamic input,
        FunctionContext context)
    {
        var log = context.GetLogger("InvoiceGetByIdAndUserIdActivity");
        var invoiceId = (Guid)input.InvoiceId;
        var userId = (Guid)input.UserId;
        log.LogInformation(
            $"[InvoiceGetByIdAndUserIdActivity] Activity started for invoice {invoiceId} and user {userId}");
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId);
        if (invoice != null && invoice.UserId == userId)
        {
            log.LogInformation($"Invoice {invoiceId} found for user {userId}");
            return invoice;
        }

        log.LogWarning($"Invoice {invoiceId} not found or does not belong to user {userId}");
        log.LogInformation(
            $"[InvoiceGetByIdAndUserIdActivity] Activity completed for invoice {invoiceId} and user {userId}");
        return null;
    }

    [Function("PatchInvoiceByCurrentUserActivity")]
    public async Task<Invoice> PatchInvoiceByCurrentUserActivity([ActivityTrigger] object input, FunctionContext context)
    {
        var log = context.GetLogger("PatchInvoiceByCurrentUserActivity");
        var inputElement = (System.Text.Json.JsonElement)input;
        var invoiceId = inputElement.GetProperty("InvoiceId").GetGuid();
        var updateRequest = System.Text.Json.JsonSerializer.Deserialize<UpdateInvoiceRequest>(inputElement.GetProperty("UpdateRequest").GetRawText());
        var userId = inputElement.GetProperty("UserId").GetGuid();
        var existing = await invoiceRepository.GetByIdAsync(invoiceId);
        if (existing == null || existing.UserId != userId)
            throw new UnauthorizedAccessException("Invoice not found or does not belong to user");
        if (existing.Status != InvoiceStatus.Pending)
            throw new InvalidOperationException("Only pending invoices can be updated");

        PatchInvoice(existing, updateRequest);

        var updated = await invoiceRepository.UpdateAsync(existing);
        await cacheService.RemoveAsync($"invoices:user:{userId}");
        await cacheService.RemoveByPatternAsync($"invoices:user:{userId}");
        await cacheService.RemoveByPatternAsync($"invoices:{existing.Id}");
        log.LogInformation("Invoice patched by current user and related cache keys invalidated");
        return updated;
    }

    private void PatchInvoice(Invoice existing, UpdateInvoiceRequest? updateRequest)
    {
        if (updateRequest == null) return;
        if (updateRequest.InvoiceNumber != null)
            existing.InvoiceNumber = updateRequest.InvoiceNumber;
        if (updateRequest.IssueDate.HasValue)
            existing.IssueDate = updateRequest.IssueDate.Value;
        if (updateRequest.ClientName != null)
            existing.ClientName = updateRequest.ClientName;
        if (updateRequest.ClientNIF != null)
            existing.ClientNIF = updateRequest.ClientNIF;
        if (updateRequest.ClientAddress != null)
            existing.ClientAddress = updateRequest.ClientAddress;
        if (updateRequest.Subtotal.HasValue)
            existing.Subtotal = updateRequest.Subtotal.Value;
        if (updateRequest.VAT.HasValue)
            existing.VAT = updateRequest.VAT.Value;
        if (updateRequest.IRPF.HasValue)
            existing.IRPF = updateRequest.IRPF.Value;
        if (updateRequest.Total.HasValue)
            existing.Total = updateRequest.Total.Value;
        if (updateRequest.Notes != null)
            existing.Notes = updateRequest.Notes;
        if (updateRequest.UserId.HasValue)
            existing.UserId = updateRequest.UserId.Value;
        if (updateRequest.Status.HasValue)
            existing.Status = updateRequest.Status.Value;
        // Items patching is not handled here; implement if needed
    }
}
