using System.Text.Json;
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

    [Function(nameof(InvoiceUpdateActivity))]
    public async Task<Invoice> InvoiceUpdateActivity([ActivityTrigger] Invoice invoice, FunctionContext context)
    {
        var log = context.GetLogger("InvoiceUpdateActivity");
        var updated = await invoiceRepository.UpdateAsync(invoice);
        await cacheService.RemoveByPatternAsync("invoices");
        log.LogInformation("Invoice updated and cache invalidated for pattern 'invoices'");
        return updated;
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

    [Function("UpdateInvoiceByCurrentUserActivity")]
    public async Task<Invoice> UpdateInvoiceByCurrentUserActivity([ActivityTrigger] object input,
        FunctionContext context)
    {
        var log = context.GetLogger("UpdateInvoiceByCurrentUserActivity");
        var inputElement = (JsonElement)input;
        var invoiceElement = inputElement.GetProperty("Invoice");
        var invoice = JsonSerializer.Deserialize<Invoice>(invoiceElement.GetRawText());
        var userId = inputElement.GetProperty("UserId").GetGuid();
        var existing = await invoiceRepository.GetByIdAsync(invoice!.Id);
        if (existing == null || existing.UserId != userId)
            throw new UnauthorizedAccessException("Invoice not found or does not belong to user");
        if (existing.Status != InvoiceStatus.Pending)
            throw new InvalidOperationException("Only pending invoices can be updated");
        // Copy properties from invoice to existing (except ID, UserId, Status)
        existing.InvoiceNumber = invoice.InvoiceNumber;
        existing.IssueDate = invoice.IssueDate;
        existing.ClientName = invoice.ClientName;
        existing.ClientNIF = invoice.ClientNIF;
        existing.ClientAddress = invoice.ClientAddress;
        existing.Subtotal = invoice.Subtotal;
        existing.VAT = invoice.VAT;
        existing.IRPF = invoice.IRPF;
        existing.Total = invoice.Total;
        existing.Notes = invoice.Notes;
        existing.Items = invoice.Items;
        // Do not update UserId or Status here
        var updated = await invoiceRepository.UpdateAsync(existing);
        // Invalidate user invoice list cache (exact key and pattern)
        await cacheService.RemoveAsync($"invoices:user:{userId}");
        await cacheService.RemoveByPatternAsync($"invoices:user:{userId}");
        await cacheService.RemoveByPatternAsync($"invoices:{existing.Id}");
        log.LogInformation("Invoice updated by current user and related cache keys invalidated");
        return updated;
    }

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
}