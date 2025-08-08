using Legal_IA.Interfaces.Repositories;
using Legal_IA.Interfaces.Services;
using Legal_IA.Models;
using Microsoft.Azure.Functions.Worker;

namespace Legal_IA.Functions.Activities;

public class InvoiceActivities(IInvoiceRepository invoiceRepository, ICacheService cacheService)
{
    [Function(nameof(InvoiceGetAllActivity))]
    public async Task<List<Invoice>> InvoiceGetAllActivity([ActivityTrigger] object input)
    {
        const string cacheKey = "invoices:all";
        var cached = await cacheService.GetAsync<List<Invoice>>(cacheKey);
        if (cached != null) return cached;
        var invoices = (await invoiceRepository.GetAllAsync()).ToList();
        await cacheService.SetAsync(cacheKey, invoices);
        return invoices;
    }

    [Function(nameof(InvoiceGetByIdActivity))]
    public async Task<Invoice?> InvoiceGetByIdActivity([ActivityTrigger] Guid id)
    {
        var cacheKey = $"invoices:{id}";
        var cached = await cacheService.GetAsync<Invoice>(cacheKey);
        if (cached != null) return cached;
        var invoice = await invoiceRepository.GetByIdAsync(id);
        if (invoice != null) await cacheService.SetAsync(cacheKey, invoice);
        return invoice;
    }

    [Function("InvoiceGetByUserIdActivity")]
    public async Task<List<Invoice>> InvoiceGetByUserIdActivity([ActivityTrigger] Guid userId)
    {
        var cacheKey = $"invoices:user:{userId}";
        var cached = await cacheService.GetAsync<List<Invoice>>(cacheKey);
        if (cached != null) return cached;
        var invoices = (await invoiceRepository.GetInvoicesByUserIdAsync(userId)).ToList();
        await cacheService.SetAsync(cacheKey, invoices);
        return invoices;
    }

    [Function(nameof(InvoiceCreateActivity))]
    public async Task<Invoice> InvoiceCreateActivity([ActivityTrigger] Invoice invoice)
    {
        if (invoice.UserId == Guid.Empty)
            throw new ArgumentException("UserId must be set on Invoice");
        var created = await invoiceRepository.AddAsync(invoice);
        await cacheService.RemoveByPatternAsync("invoices");
        return created;
    }

    [Function(nameof(InvoiceUpdateActivity))]
    public async Task<Invoice> InvoiceUpdateActivity([ActivityTrigger] Invoice invoice)
    {
        var updated = await invoiceRepository.UpdateAsync(invoice);
        await cacheService.RemoveByPatternAsync("invoices");
        return updated;
    }

    [Function(nameof(InvoiceDeleteActivity))]
    public async Task<bool> InvoiceDeleteActivity([ActivityTrigger] Guid id)
    {
        var invoice = await invoiceRepository.GetByIdAsync(id);
        var deleted = await invoiceRepository.DeleteAsync(id);
        await cacheService.RemoveAsync("invoices:all");
        await cacheService.RemoveAsync($"invoices:{id}");
        if (invoice != null)
            await cacheService.RemoveAsync($"invoices:user:{invoice.UserId}");
        return deleted;
    }

    [Function("UpdateInvoiceByCurrentUserActivity")]
    public async Task<Invoice> UpdateInvoiceByCurrentUserActivity([ActivityTrigger] dynamic input)
    {
        Invoice invoice = input.Invoice.ToObject<Invoice>();
        Guid userId = (Guid)input.UserId;
        var existing = await invoiceRepository.GetByIdAsync(invoice.Id);
        if (existing == null || existing.UserId != userId)
            throw new UnauthorizedAccessException("Invoice not found or does not belong to user");
        invoice.UserId = userId; // Ensure user cannot change owner
        var updated = await invoiceRepository.UpdateAsync(invoice);
        await cacheService.RemoveByPatternAsync($"invoices:user:{userId}");
        await cacheService.RemoveAsync($"invoices:{invoice.Id}");
        return updated;
    }
}