using System.Text.Json;
using Legal_IA.DTOs;
using Legal_IA.Interfaces.Services;
using Legal_IA.Shared.Enums;
using Legal_IA.Shared.Models;
using Legal_IA.Shared.Repositories.Interfaces;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Activities;

/// <summary>
///     Activity functions for invoice operations, including caching and repository access.
/// </summary>
public class InvoiceActivities(IInvoiceRepository invoiceRepository, ICacheService cacheService)
{
    /// <summary>
    ///     Gets all invoices, using cache if available.
    /// </summary>
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

    /// <summary>
    ///     Gets an invoice by its ID, using cache if available.
    /// </summary>
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

    /// <summary>
    ///     Gets all invoices for a specific user, using cache if available.
    /// </summary>
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

    /// <summary>
    ///     Creates a new invoice and invalidates related cache.
    /// </summary>
    [Function(nameof(InvoiceCreateActivity))]
    public async Task<Invoice> InvoiceCreateActivity([ActivityTrigger] Invoice invoice, FunctionContext context)
    {
        var log = context.GetLogger("InvoiceCreateActivity");
        if (invoice.UserId == Guid.Empty)
            throw new ArgumentException("UserId must be set on Invoice");
        var created = await invoiceRepository.AddAsync(invoice);
        await InvalidateCache(userId: invoice.UserId, invalidateAll: true, log: log);
        log.LogInformation("Invoice created and cache invalidated for user and all invoices");
        return created;
    }

    /// <summary>
    ///     Deletes an invoice and invalidates related cache.
    /// </summary>
    [Function(nameof(InvoiceDeleteActivity))]
    public async Task<bool> InvoiceDeleteActivity([ActivityTrigger] Guid id, FunctionContext context)
    {
        var log = context.GetLogger("InvoiceDeleteActivity");
        var invoice = await invoiceRepository.GetByIdAsync(id);
        var deleted = await invoiceRepository.DeleteAsync(id);
        if (invoice != null)
            await InvalidateCache(invoiceId: invoice.Id, userId: invoice.UserId, invalidateAll: true, log: log);
        log.LogInformation("Invoice deleted and related cache keys invalidated");
        return deleted;
    }

    /// <summary>
    ///     Gets an invoice by its ID and user ID.
    /// </summary>
    [Function("InvoiceGetByIdAndUserIdActivity")]
    public async Task<Invoice?> InvoiceGetByIdAndUserIdActivity([ActivityTrigger] object input,
        FunctionContext context)
    {
        var log = context.GetLogger("InvoiceGetByIdAndUserIdActivity");
        Guid invoiceId;
        Guid userId;
        if (input is JsonElement inputElement)
        {
            invoiceId = inputElement.GetProperty("InvoiceId").GetGuid();
            userId = inputElement.GetProperty("UserId").GetGuid();
        }
        else
        {
            dynamic dyn = input;
            if (dyn != null)
            {
                invoiceId = (Guid)dyn.InvoiceId;
                userId = (Guid)dyn.UserId;
            }
            else
            {
                throw new InvalidOperationException("Invalid input for InvoiceGetByIdAndUserIdActivity");
            }
        }

        log.LogInformation(
            $"[InvoiceGetByIdAndUserIdActivity] Activity started for invoice {invoiceId} and user {userId}");
        var invoice = await invoiceRepository.GetByIdAsync(invoiceId);
        if (invoice != null && invoice.UserId == userId)
        {
            log.LogInformation($"Invoice {invoiceId} found for user {userId}");
            return invoice;
        }

        log.LogWarning($"Invoice {invoiceId} not found or does not belong to user {userId}");
        return null;
    }

    /// <summary>
    ///     Patches an invoice by the current user.
    /// </summary>
    [Function("PatchInvoiceByCurrentUserActivity")]
    public async Task<Invoice> PatchInvoiceByCurrentUserActivity([ActivityTrigger] object input,
        FunctionContext context)
    {
        var log = context.GetLogger("PatchInvoiceByCurrentUserActivity");
        var inputElement = (JsonElement)input;
        var invoiceId = inputElement.GetProperty("InvoiceId").GetGuid();
        var updateRequest =
            JsonSerializer.Deserialize<UpdateInvoiceRequest>(inputElement.GetProperty("UpdateRequest").GetRawText());
        var userId = inputElement.GetProperty("UserId").GetGuid();
        var existing = await invoiceRepository.GetByIdAsync(invoiceId);
        if (existing == null || existing.UserId != userId)
            throw new UnauthorizedAccessException("Invoice not found or does not belong to user");
        if (existing.Status != InvoiceStatus.Pending)
            throw new InvalidOperationException("Only pending invoices can be updated");

        PatchInvoice(existing, updateRequest);

        var updated = await invoiceRepository.UpdateAsync(existing);

        await InvalidateCache(invoiceId: existing.Id, userId: userId, invalidateAll: true, log: log);
        log.LogInformation("Invoice patched by current user and related cache keys invalidated");
        return updated;
    }

    private async Task InvalidateCache(ILogger log, Guid? invoiceId = null, Guid? userId = null,
        bool invalidateAll = false)
    {
        if (userId.HasValue)
            await cacheService.RemoveByPatternAsync($"invoices:user:{userId.Value}");
        if (invoiceId.HasValue)
            await cacheService.RemoveByPatternAsync($"invoices:{invoiceId.Value}");
        if (invalidateAll)
            await cacheService.RemoveByPatternAsync("invoices:all");
        log.LogInformation($"Cache invalidated for invoice {invoiceId}, user {userId}, all: {invalidateAll}");
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