using System.Linq.Expressions;
using Legal_IA.Data;
using Legal_IA.Interfaces.Repositories;
using Legal_IA.Models;
using Microsoft.EntityFrameworkCore;

namespace Legal_IA.Repositories;

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
        context.InvoiceItems.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<InvoiceItem> UpdateAsync(InvoiceItem entity)
    {
        context.InvoiceItems.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var item = await context.InvoiceItems.FindAsync(id);
        if (item == null) return false;
        context.InvoiceItems.Remove(item);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Expression<Func<InvoiceItem, bool>> predicate)
    {
        return await context.InvoiceItems.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<InvoiceItem, bool>>? predicate = null)
    {
        return predicate == null
            ? await context.InvoiceItems.CountAsync()
            : await context.InvoiceItems.CountAsync(predicate);
    }

    public async Task<IEnumerable<InvoiceItem>> GetPagedAsync(int page, int pageSize,
        Expression<Func<InvoiceItem, bool>>? predicate = null)
    {
        var query = context.InvoiceItems.Include(ii => ii.Invoice).AsQueryable();
        if (predicate != null) query = query.Where(predicate);
        return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<IEnumerable<InvoiceItem>> GetItemsByInvoiceIdAsync(int invoiceId)
    {
        return await context.InvoiceItems.Where(ii => ii.InvoiceId == invoiceId).Include(ii => ii.Invoice)
            .ToListAsync();
    }

    public async Task<List<InvoiceItem>> GetByUserIdAsync(Guid userId)
    {
        return await context.InvoiceItems
            .Include(ii => ii.Invoice)
            .Where(ii => ii.Invoice.UserId == userId)
            .ToListAsync();
    }
}