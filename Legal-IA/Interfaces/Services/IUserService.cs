using Legal_IA.DTOs;
using Legal_IA.Shared.Models;

namespace Legal_IA.Interfaces.Services;

/// <summary>
///     Service interface for managing user operations
/// </summary>
public interface IUserService
{
    /// <summary>
    ///     Gets a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>The user response if found, otherwise null.</returns>
    Task<UserResponse?> GetUserByIdAsync(Guid id);

    /// <summary>
    ///     Gets a user by their email address.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>The user response if found, otherwise null.</returns>
    Task<UserResponse?> GetUserByEmailAsync(string email);

    /// <summary>
    ///     Gets all users in the system.
    /// </summary>
    /// <returns>A list of user responses.</returns>
    Task<List<UserResponse>> GetAllUsersAsync();

    /// <summary>
    ///     Creates a new user in the system.
    /// </summary>
    /// <param name="request">The user creation request.</param>
    /// <returns>The created user response.</returns>
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);

    /// <summary>
    ///     Updates an existing user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <param name="request">The update request.</param>
    /// <returns>The updated user response if found, otherwise null.</returns>
    Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request);

    /// <summary>
    ///     Deletes a user by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>True if deleted, false otherwise.</returns>
    Task<bool> DeleteUserAsync(Guid id);

    /// <summary>
    ///     Gets the user entity by their email address.
    /// </summary>
    /// <param name="email">The user's email address.</param>
    /// <returns>The user entity if found, otherwise null.</returns>
    Task<User?> GetUserEntityByEmailAsync(string email);

    /// <summary>
    ///     Gets a user entity by their email verification token.
    /// </summary>
    /// <param name="token">The email verification token.</param>
    /// <returns>The user entity if found, otherwise null.</returns>
    Task<User?> GetUserByVerificationTokenAsync(string token);

    /// <summary>
    ///     Updates the user entity in the system.
    /// </summary>
    /// <param name="user">The user entity to update.</param>
    Task UpdateUserAsync(User user);
}