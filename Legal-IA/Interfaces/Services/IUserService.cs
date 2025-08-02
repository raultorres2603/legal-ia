using Legal_IA.DTOs;

namespace Legal_IA.Interfaces.Services;

/// <summary>
///     Service interface for managing user operations
/// </summary>
public interface IUserService
{
    Task<UserResponse?> GetUserByIdAsync(Guid id);
    Task<UserResponse?> GetUserByEmailAsync(string email);
    Task<List<UserResponse>> GetAllUsersAsync();
    Task<UserResponse> CreateUserAsync(CreateUserRequest request);
    Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request);
    Task<bool> DeleteUserAsync(Guid id);
    Task<bool> UserExistsByEmailAsync(string email);
    Task<bool> UserExistsByDNIAsync(string dni);
}