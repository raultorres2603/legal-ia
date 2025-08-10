using Legal_IA.Models;

namespace Legal_IA.Interfaces.Repositories;

/// <summary>
///     Interface for user repository to manage user-related database operations
/// </summary>
public interface IUserRepository : IRepository<User>
{
    /// <summary>
    ///     Gets a user by their email address.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>The user entity if found, otherwise null.</returns>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    ///     Gets a user by their DNI (Documento Nacional de Identidad).
    /// </summary>
    /// <param name="dni">The user's DNI.</param>
    /// <returns>The user entity if found, otherwise null.</returns>
    Task<User?> GetByDNIAsync(string dni);

    /// <summary>
    ///     Gets a user by their CIF (tax identification code).
    /// </summary>
    /// <param name="cif">The user's CIF.</param>
    /// <returns>The user entity if found, otherwise null.</returns>
    Task<User?> GetByCIFAsync(string cif);

    /// <summary>
    ///     Gets all active users.
    /// </summary>
    /// <returns>A collection of active users.</returns>
    Task<IEnumerable<User>> GetActiveUsersAsync();

    /// <summary>
    ///     Gets a user by their email verification token.
    /// </summary>
    /// <param name="token">The email verification token.</param>
    /// <returns>The user entity if found, otherwise null.</returns>
    Task<User?> GetByVerificationTokenAsync(string token);
}