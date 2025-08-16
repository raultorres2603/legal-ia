using System.Linq.Expressions;
using Legal_IA.Shared.Data;
using Legal_IA.Shared.Models;
using Legal_IA.Shared.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Legal_IA.Shared.Repositories;

public class InvoiceItemRepository(LegalIaDbContext context) : IInvoiceItemRepository
{
    public async Task<InvoiceItem?> GetByIdAsync(Guid id)
    {
        return await context.InvoiceItems.Include(ii => ii.Invoice).FirstOrDefaultAsync(ii => Equals(ii.Id, id));
    }

    public async Task<IEnumerable<InvoiceItem>> GetAllAsync()
    {
        return await context.InvoiceItems.Include(ii => ii.Invoice).ToListAsync();
    }

    public async Task<IEnumerable<InvoiceItem>> FindAsync(Expression<Func<InvoiceItem, bool>> predicate)
    {
        return await context.InvoiceItems.Where(predicate).Include(ii => ii.Invoice).ToListAsync();
    }

    public async Task<InvoiceItem?> FirstOrDefaultAsync(Expression<Func<InvoiceItem, bool>> predicate)
    {
        return await context.InvoiceItems.Include(ii => ii.Invoice).FirstOrDefaultAsync(predicate);
    }

    public async Task<InvoiceItem> AddAsync(InvoiceItem entity)
    {
        var invoice = await context.Invoices.FindAsync(entity.InvoiceId);
        if (invoice == null)
            throw new ArgumentException($"Invoice with ID {entity.InvoiceId} does not exist.");
        invoice.Total += entity.Total; // Assuming Total is the cost of the item
        context.Invoices.Update(invoice);
        context.InvoiceItems.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<InvoiceItem> UpdateAsync(InvoiceItem entity)
    {
        var existingItem = await context.InvoiceItems.FindAsync(entity.Id);
        if (existingItem == null)
            throw new ArgumentException($"InvoiceItem with ID {entity.Id} does not exist.");
        var invoice = await context.Invoices.FindAsync(entity.InvoiceId);
        if (invoice == null)
            throw new ArgumentException($"Invoice with ID {entity.InvoiceId} does not exist.");
        // ...existing code...
        invoice.Total += entity.Total - existingItem.Total; // Adjust total based on the update
        context.Invoices.Update(invoice);
        context.InvoiceItems.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public Task<bool> DeleteAsync(Guid id)
    {
var invoiceItem =context.InvoiceItems.Find(id);
        if (invoiceItem == null) return Task.FromResult(false);
        
        var invoice = context.Invoices.Find(invoiceItem.InvoiceId);
        if (invoice == null) return Task.FromResult(false);
        
        invoice.Total -= invoiceItem.Total; // Adjust total based on the item being deleted
        context.Invoices.Update(invoice);
        context.InvoiceItems.Remove(invoiceItem);
        return context.SaveChangesAsync()
            .ContinueWith(t => t.Result > 0);  }

    // ...existing code...
    public Task<List<InvoiceItem>> GetItemsByInvoiceIdAsync(Guid invoiceId)
    {
        return context.InvoiceItems
            .Where(ii => ii.InvoiceId == invoiceId)
            .Include(ii => ii.Invoice)
            .ToListAsync();
    }

    public Task<List<InvoiceItem>> GetByUserIdAsync(Guid userId)
    {
        return context.InvoiceItems
            .Where(ii => ii.Invoice.UserId == userId)
            .Include(ii => ii.Invoice)
            .ToListAsync();
    }
}
