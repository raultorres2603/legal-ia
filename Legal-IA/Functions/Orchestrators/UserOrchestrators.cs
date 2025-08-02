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
            var logger = context.CreateReplaySafeLogger<UserOrchestrators>();
            logger.LogInformation("Orchestration started: UserGetAllOrchestrator");
            try
            {
                var result = await context.CallActivityAsync<List<UserResponse>>("GetAllUsersActivity", null);
                logger.LogInformation("Orchestration succeeded: UserGetAllOrchestrator, returned {Count} users.", result.Count);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in UserGetAllOrchestrator");
                throw;
            }
        }

        [Function("UserGetByIdOrchestrator")]
        public static async Task<UserResponse?> RunGetById([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var logger = context.CreateReplaySafeLogger<UserOrchestrators>();
            var userId = context.GetInput<Guid>();
            logger.LogInformation("Orchestration started: UserGetByIdOrchestrator for UserId: {UserId}", userId);
            try
            {
                var result = await context.CallActivityAsync<UserResponse?>("GetUserByIdActivity", userId);
                logger.LogInformation("Orchestration succeeded: UserGetByIdOrchestrator for UserId: {UserId}", userId);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in UserGetByIdOrchestrator for UserId: {UserId}", userId);
                throw;
            }
        }

        [Function("UserCreateOrchestrator")]
        public static async Task<string> RunCreate([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var logger = context.CreateReplaySafeLogger<UserOrchestrators>();
            var createRequest = context.GetInput<CreateUserRequest>();
            logger.LogInformation("Orchestration started: UserCreateOrchestrator for Email: {Email}", createRequest.Email);
            try
            {
                var result = await context.CallActivityAsync<string>("CreateUserActivity", createRequest);
                logger.LogInformation("Orchestration succeeded: UserCreateOrchestrator for Email: {Email}", createRequest.Email);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in UserCreateOrchestrator for Email: {Email}", createRequest.Email);
                throw;
            }
        }

        [Function("UserUpdateOrchestrator")]
        public static async Task<string> RunUpdate([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var logger = context.CreateReplaySafeLogger<UserOrchestrators>();
            var updateData = context.GetInput<dynamic>();
            logger.LogInformation("Orchestration started: UserUpdateOrchestrator for UserId: {UserId}", updateData.UserId);
            try
            {
                var result = await context.CallActivityAsync<string>("UpdateUserActivity", updateData);
                logger.LogInformation("Orchestration succeeded: UserUpdateOrchestrator for UserId: {UserId}", updateData.UserId);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in UserUpdateOrchestrator for UserId: {UserId}", updateData.UserId);
                throw;
            }
        }

        [Function("UserDeleteOrchestrator")]
        public static async Task<bool> RunDelete([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var logger = context.CreateReplaySafeLogger<UserOrchestrators>();
            var userId = context.GetInput<Guid>();
            logger.LogInformation("Orchestration started: UserDeleteOrchestrator for UserId: {UserId}", userId);
            try
            {
                var result = await context.CallActivityAsync<bool>("DeleteUserActivity", userId);
                logger.LogInformation("Orchestration succeeded: UserDeleteOrchestrator for UserId: {UserId}", userId);
                return result;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in UserDeleteOrchestrator for UserId: {UserId}", userId);
                throw;
            }
        }
    }
}
