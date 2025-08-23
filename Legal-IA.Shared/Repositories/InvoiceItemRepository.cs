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
        invoice.Total += entity.Total - existingItem.Total; // Adjust total based on the update
        context.Invoices.Update(invoice);
        context.InvoiceItems.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var item = await context.InvoiceItems.FindAsync(id);
        if (item == null) return false;

        var invoice = await context.Invoices.FindAsync(item.InvoiceId);
        if (invoice != null)
        {
            invoice.Total -= item.Total; // Adjust total when deleting item
            context.Invoices.Update(invoice);
        }

        context.InvoiceItems.Remove(item);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<List<InvoiceItem>> GetItemsByInvoiceIdAsync(Guid invoiceId)
    {
        return await context.InvoiceItems
            .Where(ii => ii.InvoiceId == invoiceId)
            .Include(ii => ii.Invoice)
            .ToListAsync();
    }

    public async Task<List<InvoiceItem>> GetByUserIdAsync(Guid userId)
    {
        return await context.InvoiceItems
            .Include(ii => ii.Invoice)
            .Where(ii => ii.Invoice.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<InvoiceItem>> GetInvoiceItemsByUserIdAsync(Guid userId)
    {
        return await GetByUserIdAsync(userId);
    }
}
