using System.Linq.Expressions;

namespace Legal_IA.Shared.Repositories.Interfaces;

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
}