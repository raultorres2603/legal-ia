using FluentValidation;
using Legal_IA.DTOs;
using Legal_IA.Interfaces.Repositories;
using Legal_IA.Interfaces.Services;
using Legal_IA.Models;
using Legal_IA.Validators;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Services;

/// <summary>
///     User service implementation using repository pattern
/// </summary>
public class UserService(IUserRepository userRepository, ICacheService cacheService, ILogger<UserService> logger)
    : IUserService
{
    private const string CacheKeyPrefix = "user:";

    public async Task<UserResponse?> GetUserByIdAsync(Guid id)
    {
        try
        {
            var cacheKey = $"{CacheKeyPrefix}{id}";
            var cachedUser = await cacheService.GetAsync<UserResponse>(cacheKey);
            if (cachedUser != null)
            {
                logger.LogInformation("Cache hit for user {UserId}", id);
                return cachedUser;
            }

            logger.LogInformation("Cache miss for user {UserId}, querying repository", id);
            var user = await userRepository.GetByIdAsync(id);
            if (user == null)
            {
                logger.LogWarning("User not found for id {UserId}", id);
                return null;
            }

            var userResponse = MapToUserResponse(user);
            await cacheService.SetAsync(cacheKey, userResponse);
            logger.LogInformation("User {UserId} cached successfully", id);
            return userResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetUserByIdAsync for id {UserId}", id);
            throw new Exception($"Error retrieving user by id {id}", ex);
        }
    }

    public async Task<UserResponse?> GetUserByEmailAsync(string email)
    {
        try
        {
            logger.LogInformation("Getting user by email: {Email}", email);
            var user = await userRepository.GetByEmailAsync(email);
            if (user != null)
            {
                logger.LogInformation("User found for email: {Email}", email);
                return MapToUserResponse(user);
            }

            logger.LogWarning("User not found for email: {Email}", email);
            return null;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetUserByEmailAsync for email {Email}", email);
            throw new Exception($"Error retrieving user by email {email}", ex);
        }
    }

    public async Task<List<UserResponse>> GetAllUsersAsync()
    {
        try
        {
            logger.LogInformation("Getting all active users");
            var users = await userRepository.GetActiveUsersAsync();
            var enumerable = users.ToList();
            logger.LogInformation("Found {Count} active users", enumerable.Count());
            return enumerable.Select(MapToUserResponse).ToList();
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error in GetAllUsersAsync");
            throw new Exception("Error retrieving active users", e);
        }
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest request)
    {
        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            Password = request.Password,
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

        var validator = new UserValidator();
        var validationResult = await validator.ValidateAsync(user);
        if (!validationResult.IsValid)
        {
            var errorMessages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            logger.LogWarning("User creation validation failed: {Errors}", errorMessages);
            throw new ValidationException(errorMessages);
        }

        var createdUser = await userRepository.AddAsync(user);
        var userResponse = MapToUserResponse(createdUser);

        var cacheKey = $"{CacheKeyPrefix}{createdUser.Id}";
        await cacheService.SetAsync(cacheKey, userResponse);

        return userResponse;
    }

    public async Task<UserResponse?> UpdateUserAsync(Guid id, UpdateUserRequest request)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user == null) return null;

        // Update fields
        if (!string.IsNullOrEmpty(request.FirstName)) user.FirstName = request.FirstName;
        if (!string.IsNullOrEmpty(request.LastName)) user.LastName = request.LastName;
        if (!string.IsNullOrEmpty(request.Email)) user.Email = request.Email;
        if (!string.IsNullOrEmpty(request.BusinessName)) user.BusinessName = request.BusinessName;
        if (!string.IsNullOrEmpty(request.CIF)) user.CIF = request.CIF;
        if (!string.IsNullOrEmpty(request.Address)) user.Address = request.Address;
        if (!string.IsNullOrEmpty(request.PostalCode)) user.PostalCode = request.PostalCode;
        if (!string.IsNullOrEmpty(request.City)) user.City = request.City;
        if (!string.IsNullOrEmpty(request.Province)) user.Province = request.Province;
        if (!string.IsNullOrEmpty(request.Phone)) user.Phone = request.Phone;
        if (request.IsActive.HasValue) user.IsActive = request.IsActive.Value;

        user.UpdatedAt = DateTime.UtcNow;

        var validator = new UserValidator();
        var validationResult = await validator.ValidateAsync(user);
        if (!validationResult.IsValid)
        {
            var errorMessages = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            logger.LogWarning("User update validation failed: {Errors}", errorMessages);
            throw new ValidationException(errorMessages);
        }

        var updatedUser = await userRepository.UpdateAsync(user);

        // Invalidate cache
        var cacheKey = $"{CacheKeyPrefix}{id}";
        await cacheService.RemoveByPatternAsync(cacheKey);

        return MapToUserResponse(updatedUser);
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await userRepository.GetByIdAsync(id);
        if (user == null) return false;

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;
        await userRepository.UpdateAsync(user);

        // Invalidate cache
        var cacheKey = $"{CacheKeyPrefix}{id}";
        await cacheService.RemoveByPatternAsync(cacheKey);

        return true;
    }

    public Task<User?> GetUserEntityByEmailAsync(string email)
    {
        return userRepository.GetByEmailAsync(email);
    }

    public async Task<bool> UserExistsByEmailAsync(string email)
    {
        return await userRepository.ExistsAsync(u => u.Email == email && u.IsActive);
    }

    public async Task<User?> GetUserByVerificationTokenAsync(string token)
    {
        try
        {
            var user = await userRepository.GetByVerificationTokenAsync(token);
            if (user == null)
            {
                logger.LogWarning("User not found for verification token {Token}", token);
                return null;
            }
            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetUserByVerificationTokenAsync for token {Token}", token);
            throw new Exception($"Error retrieving user by verification token {token}", ex);
        }
    }

    public async Task UpdateUserAsync(User user)
    {
        try
        {
            await userRepository.UpdateAsync(user);
            // Invalidate cache if needed
            var cacheKey = $"{CacheKeyPrefix}{user.Id}";
            await cacheService.RemoveAsync(cacheKey);
            logger.LogInformation("User {UserId} updated and cache invalidated", user.Id);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in UpdateUserAsync for user {UserId}", user.Id);
            throw new Exception($"Error updating user {user.Id}", ex);
        }
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