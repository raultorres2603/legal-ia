using System.Diagnostics;
using Legal_IA.DTOs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace Legal_IA.Functions.Orchestrators;

public static class UserOrchestrators
{
    [Function("UserGetAllOrchestrator")]
    public static async Task<List<UserResponse>> RunGetAll([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        Trace.TraceInformation("Orchestration started: UserGetAllOrchestrator");
        try
        {
            var result = await context.CallActivityAsync<List<UserResponse>>("GetAllUsersActivity", null);
            Trace.TraceInformation($"Orchestration succeeded: UserGetAllOrchestrator, returned {result.Count} users.");
            return result;
        }
        catch (Exception ex)
        {
            Trace.TraceError($"Error in UserGetAllOrchestrator: {ex}");
            throw;
        }
        finally
        {
            Trace.TraceInformation("Orchestration finished: UserGetAllOrchestrator");
        }
    }

    [Function("UserGetByIdOrchestrator")]
    public static async Task<UserResponse?> RunGetById([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        Trace.TraceInformation("Orchestration started: UserGetByIdOrchestrator");
        var userId = context.GetInput<Guid>();
        try
        {
            var result = await context.CallActivityAsync<UserResponse?>("GetUserByIdActivity", userId);
            Trace.TraceInformation($"Orchestration succeeded: UserGetByIdOrchestrator, returned user: {result?.Id}");
            return result;
        }
        catch (Exception ex)
        {
            Trace.TraceError($"Error in UserGetByIdOrchestrator: {ex}");
            throw;
        }
        finally
        {
            Trace.TraceInformation("Orchestration finished: UserGetByIdOrchestrator");
        }
    }

    [Function("UserCreateOrchestrator")]
    public static async Task<UserResponse?> RunCreate([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        Trace.TraceInformation("Orchestration started: UserCreateOrchestrator");
        var createRequest = context.GetInput<CreateUserRequest>();
        try
        {
            var result = await context.CallActivityAsync<UserResponse?>("CreateUserActivity", createRequest);
            Trace.TraceInformation($"Orchestration succeeded: UserCreateOrchestrator, created user: {result?.Id}");
            return result;
        }
        catch (Exception ex)
        {
            Trace.TraceError($"Error in UserCreateOrchestrator: {ex}");
            throw;
        }
        finally
        {
            Trace.TraceInformation("Orchestration finished: UserCreateOrchestrator");
        }
    }

    [Function("UserUpdateOrchestrator")]
    public static async Task<UserResponse?> RunUpdate([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        Trace.TraceInformation("Orchestration started: UserUpdateOrchestrator");
        var updateData = context.GetInput<dynamic>();
        try
        {
            var result = await context.CallActivityAsync<UserResponse?>("UpdateUserActivity", updateData);
            Trace.TraceInformation($"Orchestration succeeded: UserUpdateOrchestrator, updated user: {result?.Id}");
            return result;
        }
        catch (Exception ex)
        {
            Trace.TraceError($"Error in UserUpdateOrchestrator: {ex}");
            throw;
        }
        finally
        {
            Trace.TraceInformation("Orchestration finished: UserUpdateOrchestrator");
        }
    }

    [Function("UserDeleteOrchestrator")]
    public static async Task<bool> RunDelete([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        Trace.TraceInformation("Orchestration started: UserDeleteOrchestrator");
        var userId = context.GetInput<Guid>();
        try
        {
            var result = await context.CallActivityAsync<bool>("DeleteUserActivity", userId);
            Trace.TraceInformation(
                $"Orchestration succeeded: UserDeleteOrchestrator, deleted user: {userId}, result: {result}");
            return result;
        }
        catch (Exception ex)
        {
            Trace.TraceError($"Error in UserDeleteOrchestrator: {ex}");
            throw;
        }
        finally
        {
            Trace.TraceInformation("Orchestration finished: UserDeleteOrchestrator");
        }
    }
}