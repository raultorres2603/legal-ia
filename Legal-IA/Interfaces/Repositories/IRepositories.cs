using Legal_IA.Models;
using System.Linq.Expressions;

namespace Legal_IA.Interfaces.Repositories;

/// <summary>
/// Generic repository interface for common database operations
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
/// User-specific repository interface
/// </summary>
public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByDNIAsync(string dni);
    Task<User?> GetByCIFAsync(string cif);
    Task<IEnumerable<User>> GetActiveUsersAsync();
}

/// <summary>
/// Document-specific repository interface
/// </summary>
public interface IDocumentRepository : IRepository<Document>
{
    Task<IEnumerable<Document>> GetByUserIdAsync(Guid userId);
    Task<IEnumerable<Document>> GetByTypeAsync(DocumentType type);
    Task<IEnumerable<Document>> GetByStatusAsync(DocumentStatus status);
    Task<IEnumerable<Document>> GetTemplatesAsync();
    Task<IEnumerable<Document>> SearchAsync(string searchTerm, Guid? userId = null);
    Task<IEnumerable<Document>> GetByQuarterAndYearAsync(int quarter, int year);
    Task<IEnumerable<Document>> GetRecentDocumentsAsync(Guid userId, int count = 10);
}
