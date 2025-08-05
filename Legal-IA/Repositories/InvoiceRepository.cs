using System.Linq.Expressions;
using Legal_IA.Data;
using Legal_IA.Interfaces.Repositories;
using Legal_IA.Models;
using Microsoft.EntityFrameworkCore;

namespace Legal_IA.Repositories;

public class InvoiceRepository(LegalIADbContext context) : IInvoiceRepository
{
    public async Task<Invoice?> GetByIdAsync(Guid id)
    {
        return await context.Invoices.Include(i => i.Items).FirstOrDefaultAsync(i => Equals(i.Id, id));
    }

    public async Task<IEnumerable<Invoice>> GetAllAsync()
    {
        return await context.Invoices.Include(i => i.Items).ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> FindAsync(Expression<Func<Invoice, bool>> predicate)
    {
        return await context.Invoices.Where(predicate).Include(i => i.Items).ToListAsync();
    }

    public async Task<Invoice?> FirstOrDefaultAsync(Expression<Func<Invoice, bool>> predicate)
    {
        return await context.Invoices.Include(i => i.Items).FirstOrDefaultAsync(predicate);
    }

    public async Task<Invoice> AddAsync(Invoice entity)
    {
        context.Invoices.Add(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<Invoice> UpdateAsync(Invoice entity)
    {
        context.Invoices.Update(entity);
        await context.SaveChangesAsync();
        return entity;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var invoice = await context.Invoices.FindAsync(id);
        if (invoice == null) return false;
        context.Invoices.Remove(invoice);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ExistsAsync(Expression<Func<Invoice, bool>> predicate)
    {
        return await context.Invoices.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<Invoice, bool>>? predicate = null)
    {
        return predicate == null ? await context.Invoices.CountAsync() : await context.Invoices.CountAsync(predicate);
    }

    public async Task<IEnumerable<Invoice>> GetPagedAsync(int page, int pageSize,
        Expression<Func<Invoice, bool>>? predicate = null)
    {
        var query = context.Invoices.Include(i => i.Items).AsQueryable();
        if (predicate != null) query = query.Where(predicate);
        return await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task<IEnumerable<Invoice>> GetInvoicesByClientNIFAsync(string clientNIF)
    {
        return await context.Invoices.Where(i => i.ClientNIF == clientNIF).Include(i => i.Items).ToListAsync();
    }
}