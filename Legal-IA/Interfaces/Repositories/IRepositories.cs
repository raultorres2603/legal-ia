using System.Linq.Expressions;
using Legal_IA.Models;

namespace Legal_IA.Interfaces.Repositories;

/// <summary>
///     Generic repository interface for common database operations
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<T> AddAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
    Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? predicate = null);
}

/// <summary>
///     User-specific repository interface
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByDNIAsync(string dni);
    Task<User?> GetByCIFAsync(string cif);
    Task<IEnumerable<User>> GetActiveUsersAsync();
}

/// <summary>
///     Invoice-specific repository interface
/// </summary>
public interface IInvoiceRepository : IRepository<Invoice>
{
    Task<List<Invoice>> GetInvoicesByUserIdAsync(Guid userId);
}

/// <summary>
///     InvoiceItem-specific repository interface
/// </summary>
public interface IInvoiceItemRepository : IRepository<InvoiceItem>
{
    Task<IEnumerable<InvoiceItem>> GetItemsByInvoiceIdAsync(int invoiceId);
    Task<IEnumerable<InvoiceItem>> GetByUserIdAsync(Guid userId);
}