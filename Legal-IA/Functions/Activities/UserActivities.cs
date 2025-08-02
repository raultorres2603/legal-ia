using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Legal_IA.DTOs;
using Legal_IA.Interfaces.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Activities
{
    public class GetAllUsersActivity(IUserService userService, ILogger<GetAllUsersActivity> logger)
    {
        [Function("GetAllUsersActivity")]
        public async Task<List<UserResponse>> Run([ActivityTrigger] object? input)
        {
            logger.LogInformation("Starting GetAllUsersActivity");
            try
            {
                var result = await userService.GetAllUsersAsync();
                logger.LogInformation("GetAllUsersActivity succeeded. Returned {Count} users.", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetAllUsersActivity");
                throw;
            }
        }
    }

    public class GetUserByIdActivity(IUserService userService, ILogger<GetUserByIdActivity> logger)
    {
        [Function("GetUserByIdActivity")]
        public async Task<UserResponse?> Run([ActivityTrigger] Guid userId)
        {
            logger.LogInformation("Starting GetUserByIdActivity for UserId: {UserId}", userId);
            try
            {
                var result = await userService.GetUserByIdAsync(userId);
                if (result == null)
                {
                    logger.LogWarning("User not found for UserId: {UserId}", userId);
                    return null;
                }
                logger.LogInformation("GetUserByIdActivity succeeded for UserId: {UserId}", userId);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetUserByIdActivity for UserId: {UserId}", userId);
                throw;
            }
        }
    }

    public class CreateUserActivity(IUserService userService, ILogger<CreateUserActivity> logger)
    {
        [Function("CreateUserActivity")]
        public async Task<UserResponse> Run([ActivityTrigger] CreateUserRequest request)
        {
            logger.LogInformation("Starting CreateUserActivity for Email: {Email}", request.Email);
            try
            {
                var result = await userService.CreateUserAsync(request);
                logger.LogInformation("CreateUserActivity succeeded for Email: {Email}", request.Email);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in CreateUserActivity for Email: {Email}", request.Email);
                throw;
            }
        }
    }

    public class UpdateUserActivity(IUserService userService, ILogger<UpdateUserActivity> logger)
    {
        [Function("UpdateUserActivity")]
        public async Task<UserResponse?> Run([ActivityTrigger] UpdateUserOrchestrationInput updateData)
        {
            logger.LogInformation($"Starting UpdateUserActivity for UserId: {updateData.UserId}");
            try
            {
                var result = await userService.UpdateUserAsync(updateData.UserId, updateData.UpdateRequest);
                if (result == null)
                {
                    logger.LogWarning($"User not found for UserId: {updateData.UserId}");
                    return null;
                }
                logger.LogInformation($"User updated successfully for UserId: {updateData.UserId}");
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Error in UpdateUserActivity for UserId: {updateData.UserId}");
                throw;
            }
        }
    }

    public class DeleteUserActivity(IUserService userService, ILogger<DeleteUserActivity> logger)
    {
        [Function("DeleteUserActivity")]
        public async Task<bool> Run([ActivityTrigger] Guid userId)
        {
            logger.LogInformation("Starting DeleteUserActivity for UserId: {UserId}", userId);
            try
            {
                var result = await userService.DeleteUserAsync(userId);
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
}
