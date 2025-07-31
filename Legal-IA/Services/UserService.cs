using Legal_IA.DTOs;
using Legal_IA.Interfaces.Repositories;
using Legal_IA.Interfaces.Services;
using Legal_IA.Models;

namespace Legal_IA.Services;

/// <summary>
///     User service implementation using repository pattern
/// </summary>
public class UserService : IUserService
{
    private readonly string _cacheKeyPrefix = "user:";
    private readonly ICacheService _cacheService;
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository, ICacheService cacheService)
    {
        _userRepository = userRepository;
        _cacheService = cacheService;
    }

    public async Task<UserResponse?> GetUserByIdAsync(Guid id)
    {
        var cacheKey = $"{_cacheKeyPrefix}{id}";
        var cachedUser = await _cacheService.GetAsync<UserResponse>(cacheKey);

        if (cachedUser != null)
            return cachedUser;

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        var userResponse = MapToUserResponse(user);
        await _cacheService.SetAsync(cacheKey, userResponse);

        return userResponse;
    }

    public async Task<UserResponse?> GetUserByEmailAsync(string email)
    {
        var user = await _userRepository.GetByEmailAsync(email);
        return user != null ? MapToUserResponse(user) : null;
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync()
    {
        var users = await _userRepository.GetActiveUsersAsync();
        return users.Select(MapToUserResponse);
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            DNI = request.DNI,
            CIF = request.CIF,
            BusinessName = request.BusinessName,
            Address = request.Address,
            PostalCode = request.PostalCode,
            City = request.City,
            Province = request.Province,
            Phone = request.Phone,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdUser = await _userRepository.AddAsync(user);
        var userResponse = MapToUserResponse(createdUser);

        var cacheKey = $"{_cacheKeyPrefix}{createdUser.Id}";
        await _cacheService.SetAsync(cacheKey, userResponse);

        return userResponse;
    }

    public async Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return null;

        // Update fields
        if (!string.IsNullOrEmpty(request.FirstName)) user.FirstName = request.FirstName;
        if (!string.IsNullOrEmpty(request.LastName)) user.LastName = request.LastName;
        if (!string.IsNullOrEmpty(request.Email)) user.Email = request.Email;
        if (!string.IsNullOrEmpty(request.BusinessName)) user.BusinessName = request.BusinessName;
        if (!string.IsNullOrEmpty(request.Address)) user.Address = request.Address;
        if (!string.IsNullOrEmpty(request.PostalCode)) user.PostalCode = request.PostalCode;
        if (!string.IsNullOrEmpty(request.City)) user.City = request.City;
        if (!string.IsNullOrEmpty(request.Province)) user.Province = request.Province;
        if (!string.IsNullOrEmpty(request.Phone)) user.Phone = request.Phone;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        var updatedUser = await _userRepository.UpdateAsync(user);

        // Invalidate cache
        var cacheKey = $"{_cacheKeyPrefix}{id}";
        await _cacheService.RemoveAsync(cacheKey);

        return MapToUserResponse(updatedUser);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Invalidate cache
        var cacheKey = $"{_cacheKeyPrefix}{id}";
        await _cacheService.RemoveAsync(cacheKey);

        return true;
    }

    public async Task<bool> UserExistsByEmailAsync(string email)
    {
        return await _userRepository.ExistsAsync(u => u.Email == email && u.IsActive);
    }

    public async Task<bool> UserExistsByDNIAsync(string dni)
    {
        return await _userRepository.ExistsAsync(u => u.DNI == dni && u.IsActive);
    }

    private static UserResponse MapToUserResponse(User user)
    {
        return new UserResponse
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            DNI = user.DNI,
            CIF = user.CIF,
            BusinessName = user.BusinessName,
            Address = user.Address,
            PostalCode = user.PostalCode,
            City = user.City,
            Province = user.Province,
            Phone = user.Phone,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            IsActive = user.IsActive
        };
    }
}