using System.Linq.Expressions;

namespace Legal_IA.Interfaces.Repositories;

/// <summary>
///     Generic repository interface for common database operations
/// </summary>
public interface IRepository<T> where T : class
{
    /// <summary>
    ///     Gets an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>The entity if found, otherwise null.</returns>
    Task<T?> GetByIdAsync(Guid id);

    /// <summary>
    ///     Gets all entities of type T.
    /// </summary>
    /// <returns>An enumerable of all entities.</returns>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    ///     Finds entities matching the given predicate.
    /// </summary>
    /// <param name="predicate">The filter expression.</param>
    /// <returns>An enumerable of matching entities.</returns>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    ///     Gets the first entity matching the predicate, or null if none found.
    /// </summary>
    /// <param name="predicate">The filter expression.</param>
    /// <returns>The first matching entity, or null.</returns>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    ///     Adds a new entity to the repository.
    /// </summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>The added entity.</returns>
    Task<T> AddAsync(T entity);

    /// <summary>
    ///     Updates an existing entity in the repository.
    /// </summary>
    /// <param name="entity">The entity to update.</param>
    /// <returns>The updated entity.</returns>
    Task<T> UpdateAsync(T entity);

    /// <summary>
    ///     Deletes an entity by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the entity.</param>
    /// <returns>True if deleted, false otherwise.</returns>
    Task<bool> DeleteAsync(Guid id);

    /// <summary>
    ///     Checks if any entity exists matching the predicate.
    /// </summary>
    /// <param name="predicate">The filter expression.</param>
    /// <returns>True if any entity exists, false otherwise.</returns>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);

    /// <summary>
    ///     Counts the number of entities matching the predicate, or all if predicate is null.
    /// </summary>
    /// <param name="predicate">The filter expression (optional).</param>
    /// <returns>The count of entities.</returns>
    Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);

    /// <summary>
    ///     Gets a paged list of entities matching the predicate, or all if predicate is null.
    /// </summary>
    /// <param name="page">The page number (1-based).</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="predicate">The filter expression (optional).</param>
    /// <returns>An enumerable of entities for the page.</returns>
    Task<IEnumerable<T>> GetPagedAsync(int page, int pageSize, Expression<Func<T, bool>>? predicate = null);
}