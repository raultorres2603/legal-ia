using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Legal_IA.DTOs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Orchestrators
{
    public static class UserOrchestrators
    {
        [Function("UserGetAllOrchestrator")]
        public static async Task<List<UserResponse>> RunGetAll([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            System.Diagnostics.Trace.TraceInformation("Orchestration started: UserGetAllOrchestrator");
            try
            {
                var result = await context.CallActivityAsync<List<UserResponse>>("GetAllUsersActivity", null);
                System.Diagnostics.Trace.TraceInformation($"Orchestration succeeded: UserGetAllOrchestrator, returned {result.Count} users.");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in UserGetAllOrchestrator: {ex}");
                throw;
            }
        }

        [Function("UserGetByIdOrchestrator")]
        public static async Task<UserResponse?> RunGetById([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var userId = context.GetInput<Guid>();
            System.Diagnostics.Trace.TraceInformation($"Orchestration started: UserGetByIdOrchestrator for UserId: {userId}");
            try
            {
                var result = await context.CallActivityAsync<UserResponse?>("GetUserByIdActivity", userId);
                System.Diagnostics.Trace.TraceInformation($"Orchestration succeeded: UserGetByIdOrchestrator for UserId: {userId}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in UserGetByIdOrchestrator for UserId: {userId}: {ex}");
                throw;
            }
        }

        [Function("UserCreateOrchestrator")]
        public static async Task<UserResponse> RunCreate([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var createRequest = context.GetInput<CreateUserRequest>();
            System.Diagnostics.Trace.TraceInformation($"Orchestration started: UserCreateOrchestrator for Email: {createRequest!.Email}");
            try
            {
                var result = await context.CallActivityAsync<UserResponse>("CreateUserActivity", createRequest);
                System.Diagnostics.Trace.TraceInformation($"Orchestration succeeded: UserCreateOrchestrator for Email: {createRequest.Email}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in UserCreateOrchestrator for Email: {createRequest.Email}: {ex}");
                throw;
            }
        }

        [Function("UserUpdateOrchestrator")]
        public static async Task<string> RunUpdate([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var updateData = context.GetInput<dynamic>();
            System.Diagnostics.Trace.TraceInformation($"Orchestration started: UserUpdateOrchestrator for UserId: {updateData.UserId}");
            try
            {
                var result = await context.CallActivityAsync<string>("UpdateUserActivity", updateData);
                System.Diagnostics.Trace.TraceInformation($"Orchestration succeeded: UserUpdateOrchestrator for UserId: {updateData.UserId}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in UserUpdateOrchestrator for UserId: {updateData.UserId}: {ex}");
                throw;
            }
        }

        [Function("UserDeleteOrchestrator")]
        public static async Task<bool> RunDelete([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var userId = context.GetInput<Guid>();
            System.Diagnostics.Trace.TraceInformation($"Orchestration started: UserDeleteOrchestrator for UserId: {userId}");
            try
            {
                var result = await context.CallActivityAsync<bool>("DeleteUserActivity", userId);
                System.Diagnostics.Trace.TraceInformation($"Orchestration succeeded: UserDeleteOrchestrator for UserId: {userId}");
                return result;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error in UserDeleteOrchestrator for UserId: {userId}: {ex}");
                throw;
            }
        }
    }
}
