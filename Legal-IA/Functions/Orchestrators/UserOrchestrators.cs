using Legal_IA.DTOs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Orchestrators;

public static class UserOrchestrators
{
    [Function("UserGetAllOrchestrator")]
    public static async Task<List<UserResponse>> RunGetAll([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("UserGetAllOrchestrator");
        logger.LogInformation("[UserGetAllOrchestrator] Orchestrator started");
        try
        {
            var result = await context.CallActivityAsync<List<UserResponse>>("GetAllUsersActivity", null);
            logger.LogInformation($"[UserGetAllOrchestrator] Orchestrator completed, returned {result.Count} users");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[UserGetAllOrchestrator] Error");
            throw;
        }
    }

    [Function("UserGetByIdOrchestrator")]
    public static async Task<UserResponse?> RunGetById([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("UserGetByIdOrchestrator");
        var userId = context.GetInput<Guid>();
        logger.LogInformation($"[UserGetByIdOrchestrator] Orchestrator started for id {userId}");
        try
        {
            var result = await context.CallActivityAsync<UserResponse?>("GetUserByIdActivity", userId);
            logger.LogInformation($"[UserGetByIdOrchestrator] Orchestrator completed, returned user: {result?.Id}");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[UserGetByIdOrchestrator] Error");
            throw;
        }
    }

    [Function("UserCreateOrchestrator")]
    public static async Task<UserResponse?> RunCreate([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("UserCreateOrchestrator");
        var createRequest = context.GetInput<CreateUserRequest>();
        logger.LogInformation("[UserCreateOrchestrator] Orchestrator started");
        try
        {
            var result = await context.CallActivityAsync<UserResponse?>("CreateUserActivity", createRequest);
            logger.LogInformation($"[UserCreateOrchestrator] Orchestrator completed, created user: {result?.Id}");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[UserCreateOrchestrator] Error");
            throw;
        }
    }

    [Function("UserUpdateOrchestrator")]
    public static async Task<UserResponse?> RunUpdate([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("UserUpdateOrchestrator");
        var updateData = context.GetInput<dynamic>();
        logger.LogInformation("[UserUpdateOrchestrator] Orchestrator started");
        try
        {
            var result = await context.CallActivityAsync<UserResponse?>("UpdateUserActivity", updateData);
            logger.LogInformation($"[UserUpdateOrchestrator] Orchestrator completed, updated user: {result?.Id}");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[UserUpdateOrchestrator] Error");
            throw;
        }
    }

    [Function("UserDeleteOrchestrator")]
    public static async Task<bool> RunDelete([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("UserDeleteOrchestrator");
        var userId = context.GetInput<Guid>();
        logger.LogInformation("[UserDeleteOrchestrator] Orchestrator started");
        try
        {
            var result = await context.CallActivityAsync<bool>("DeleteUserActivity", userId);
            logger.LogInformation(
                $"[UserDeleteOrchestrator] Orchestrator completed, deleted user: {userId}, result: {result}");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[UserDeleteOrchestrator] Error");
            throw;
        }
    }

    [Function("VerifyUserEmailOrchestrator")]
    public static async Task<AuthResponse> RunVerifyEmail([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("UserVerifyEmailOrchestrator");
        var token = context.GetInput<string>();
        logger.LogInformation($"[UserVerifyEmailOrchestrator] Orchestrator started for token {token}");
        try
        {
            var result = await context.CallActivityAsync<AuthResponse>("VerifyUserEmailActivity", token);
            logger.LogInformation($"[UserVerifyEmailOrchestrator] Orchestrator completed for token {token}");
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[UserVerifyEmailOrchestrator] Error");
            return new AuthResponse { Success = false, Message = "Verification failed due to an internal error." };
        }
    }
}