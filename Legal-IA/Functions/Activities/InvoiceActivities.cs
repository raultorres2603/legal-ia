using Legal_IA.Interfaces.Repositories;
using Legal_IA.Models;
using Microsoft.Azure.Functions.Worker;

namespace Legal_IA.Functions.Activities;

public class InvoiceActivities
{
    private readonly IInvoiceRepository _invoiceRepository;

    public InvoiceActivities(IInvoiceRepository invoiceRepository)
    {
        _invoiceRepository = invoiceRepository;
    }

    [Function(nameof(InvoiceGetAllActivity))]
    public async Task<List<Invoice>> InvoiceGetAllActivity([ActivityTrigger] object input)
    {
        var invoices = await _invoiceRepository.GetAllAsync();
        return invoices.ToList();
    }

    [Function(nameof(InvoiceGetByIdActivity))]
    public async Task<Invoice?> InvoiceGetByIdActivity([ActivityTrigger] Guid id)
    {
        return await _invoiceRepository.GetByIdAsync(id);
    }

    [Function(nameof(InvoiceCreateActivity))]
    public async Task<Invoice> InvoiceCreateActivity([ActivityTrigger] Invoice invoice)
    {
        return await _invoiceRepository.AddAsync(invoice);
    }

    [Function(nameof(InvoiceUpdateActivity))]
    public async Task<Invoice> InvoiceUpdateActivity([ActivityTrigger] Invoice invoice)
    {
        return await _invoiceRepository.UpdateAsync(invoice);
    }

    [Function(nameof(InvoiceDeleteActivity))]
    public async Task<bool> InvoiceDeleteActivity([ActivityTrigger] Guid id)
    {
        return await _invoiceRepository.DeleteAsync(id);
    }
}