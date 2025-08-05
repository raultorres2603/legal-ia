using Legal_IA.Interfaces.Repositories;
using Legal_IA.Models;
using Microsoft.Azure.Functions.Worker;

namespace Legal_IA.Functions.Activities;

public class InvoiceItemActivities
{
    private readonly IInvoiceItemRepository _invoiceItemRepository;

    public InvoiceItemActivities(IInvoiceItemRepository invoiceItemRepository)
    {
        _invoiceItemRepository = invoiceItemRepository;
    }

    [Function(nameof(InvoiceItemGetAllActivity))]
    public async Task<List<InvoiceItem>> InvoiceItemGetAllActivity([ActivityTrigger] object input)
    {
        var items = await _invoiceItemRepository.GetAllAsync();
        return items.ToList();
    }

    [Function(nameof(InvoiceItemGetByIdActivity))]
    public async Task<InvoiceItem?> InvoiceItemGetByIdActivity([ActivityTrigger] Guid id)
    {
        return await _invoiceItemRepository.GetByIdAsync(id);
    }

    [Function(nameof(InvoiceItemCreateActivity))]
    public async Task<InvoiceItem> InvoiceItemCreateActivity([ActivityTrigger] InvoiceItem item)
    {
        return await _invoiceItemRepository.AddAsync(item);
    }

    [Function(nameof(InvoiceItemUpdateActivity))]
    public async Task<InvoiceItem> InvoiceItemUpdateActivity([ActivityTrigger] InvoiceItem item)
    {
        return await _invoiceItemRepository.UpdateAsync(item);
    }

    [Function(nameof(InvoiceItemDeleteActivity))]
    public async Task<bool> InvoiceItemDeleteActivity([ActivityTrigger] Guid id)
    {
        return await _invoiceItemRepository.DeleteAsync(id);
    }
}