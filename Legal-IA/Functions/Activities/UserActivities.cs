using Legal_IA.DTOs;
using Legal_IA.Interfaces.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Activities;

/// <summary>
/// Activity for user-related operations such as get, create, and patch.
/// </summary>
public class GetAllUsersActivity(IUserService userService, ILogger<GetAllUsersActivity> logger)
{
    /// <summary>
    /// Gets all users.
    /// </summary>
    [Function("GetAllUsersActivity")]
    public async Task<List<UserResponse>> Run([ActivityTrigger] object? input)
    {
        logger.LogInformation("[GetAllUsersActivity] Activity started");
        try
        {
            var result = await userService.GetAllUsersAsync();
            logger.LogInformation($"[GetAllUsersActivity] Activity completed. Returned {result.Count} users.");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetAllUsersActivity");
            throw;
        }
    }
}

/// <summary>
/// Activity for getting a user by ID.
/// </summary>
public class GetUserByIdActivity(IUserService userService, ILogger<GetUserByIdActivity> logger)
{
    /// <summary>
    /// Gets a user by their ID.
    /// </summary>
    [Function("GetUserByIdActivity")]
    public async Task<UserResponse?> Run([ActivityTrigger] Guid userId)
    {
        logger.LogInformation($"[GetUserByIdActivity] Activity started for UserId: {userId}");
        try
        {
            var result = await userService.GetUserByIdAsync(userId);
            if (result == null)
            {
                logger.LogWarning("User not found for UserId: {UserId}", userId);
                return null;
            }
            logger.LogInformation($"[GetUserByIdActivity] Activity completed for UserId: {userId}");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in GetUserByIdActivity for UserId: {UserId}", userId);
            throw;
        }
    }
}

/// <summary>
/// Activity for creating a new user.
/// </summary>
public class CreateUserActivity(
    IUserService userService,
    ICacheService cacheService,
    ILogger<CreateUserActivity> logger)
{
    /// <summary>
    /// Creates a new user and invalidates user cache.
    /// </summary>
    [Function("CreateUserActivity")]
    public async Task<UserResponse> Run([ActivityTrigger] CreateUserRequest request)
    {
        logger.LogInformation($"[CreateUserActivity] Activity started for Email: {request.Email}");
        try
        {
            var result = await userService.CreateUserAsync(request);
            await cacheService.RemoveByPatternAsync("users");
            logger.LogInformation($"[CreateUserActivity] Activity completed for Email: {request.Email}");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in CreateUserActivity for Email: {Email}", request.Email);
            throw;
        }
    }
}

/// <summary>
/// Activity for patching (updating) a user.
/// </summary>
public class PatchUserActivity(
    IUserService userService,
    ICacheService cacheService,
    ILogger<PatchUserActivity> logger)
{
    /// <summary>
    /// Updates a user and invalidates user cache.
    /// </summary>
    [Function("PatchUserActivity")]
    public async Task<UserResponse?> Run([ActivityTrigger] UpdateUserOrchestrationInput updateData)
    {
        logger.LogInformation($"Starting PatchUserActivity for UserId: {updateData.UserId}");
        try
        {
            var result = await userService.UpdateUserAsync(updateData.UserId, updateData.UpdateRequest);
            if (result == null)
            {
                logger.LogWarning($"User not found for UserId: {updateData.UserId}");
                return null;
            }
            await cacheService.RemoveByPatternAsync("users");
            logger.LogInformation($"User patched successfully for UserId: {updateData.UserId}");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in PatchUserActivity for UserId: {UserId}", updateData.UserId);
            throw;
        }
    }
}

public class DeleteUserActivity(
    IUserService userService,
    ICacheService cacheService,
    ILogger<DeleteUserActivity> logger)
{
    [Function("DeleteUserActivity")]
    public async Task<bool> Run([ActivityTrigger] Guid userId)
    {
        logger.LogInformation("Starting DeleteUserActivity for UserId: {UserId}", userId);
        try
        {
            var result = await userService.DeleteUserAsync(userId);
            await cacheService.RemoveByPatternAsync("users");
            logger.LogInformation("DeleteUserActivity succeeded for UserId: {UserId}", userId);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in DeleteUserActivity for UserId: {UserId}", userId);
            throw;
        }
    }
}