using Legal_IA.DTOs;
using Legal_IA.Enums;
using Legal_IA.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Legal_IA.Functions;

public class UserHttpTriggers(ILogger<UserHttpTriggers> logger, IConfiguration configuration)
{
    [Function("GetUsers")]
    public async Task<IActionResult> GetUsers(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.Admin)))
            return new UnauthorizedResult();
        try
        {
            var users = await client.ScheduleNewOrchestrationInstanceAsync(
                "UserGetAllOrchestrator", null);
            var response = await client.WaitForInstanceCompletionAsync(users, true, CancellationToken.None);
            return new OkObjectResult(response.ReadOutputAs<List<UserResponse>>());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting users");
            return new StatusCodeResult(500);
        }
    }

    [Function("GetUser")]
    public async Task<IActionResult> GetUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/{id}")]
        HttpRequestData req,
        string id,
        [DurableClient] DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.Admin)))
            return new UnauthorizedResult();
        try
        {
            if (!Guid.TryParse(id, out var userId)) return new BadRequestObjectResult("Invalid user ID format");
            var orchestrationId = await client.ScheduleNewOrchestrationInstanceAsync(
                "UserGetByIdOrchestrator", userId);
            var response = await client.WaitForInstanceCompletionAsync(orchestrationId, true, CancellationToken.None);
            if (response.RuntimeStatus == OrchestrationRuntimeStatus.Failed)
            {
                logger.LogError("Failed to get user {UserId}: {Error}", id, response.FailureDetails);
                return new StatusCodeResult(500);
            }

            if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            {
                // Use GetOutput<T>() to deserialize output to UserResponse
                var userResponse = response.ReadOutputAs<UserResponse>();
                if (userResponse == null)
                {
                    logger.LogWarning("User not found for id {UserId}", id);
                    return new NotFoundResult();
                }

                return new OkObjectResult(userResponse);
            }

            return new StatusCodeResult(500);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user {UserId}", id);
            return new StatusCodeResult(500);
        }
    }

    [Function("CreateUser")]
    public async Task<IActionResult> CreateUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.Admin)))
            return new UnauthorizedResult();
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var createRequest = JsonConvert.DeserializeObject<CreateUserRequest>(requestBody);

            if (createRequest == null)
                return new BadRequestObjectResult("Invalid request body");

            var result = await client.ScheduleNewOrchestrationInstanceAsync(
                "UserCreateOrchestrator", createRequest);
            var response = await client.WaitForInstanceCompletionAsync(result, true, CancellationToken.None);
            if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            {
                if (response.SerializedOutput == null && response.FailureDetails != null)
                {
                    logger.LogError("User creation failed: {Error}", response.FailureDetails);
                    return new ConflictObjectResult(response.SerializedOutput ??
                                                    "User creation failed due to a conflict or error.");
                }

                return new OkObjectResult(new
                {
                    message = "User created successfully",
                    instanceId = response.ReadOutputAs<UserResponse>()
                });
            }

            if (response.RuntimeStatus == OrchestrationRuntimeStatus.Failed)
                return new ConflictObjectResult("User creation failed due to a conflict or error.");
            return new StatusCodeResult(500);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user");
            return new StatusCodeResult(500);
        }
    }

    [Function("UpdateUser")]
    public async Task<IActionResult> UpdateUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "users/{id}")]
        HttpRequestData req,
        string id,
        [DurableClient] DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.Admin)))
            return new UnauthorizedResult();
        try
        {
            if (!Guid.TryParse(id, out var userId))
                return new BadRequestObjectResult("Invalid user ID format");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var updateRequest = JsonConvert.DeserializeObject<UpdateUserRequest>(requestBody);

            if (updateRequest == null)
                return new BadRequestObjectResult("Invalid request body");

            var updateData = new { UserId = userId, UpdateRequest = updateRequest };
            var result = await client.ScheduleNewOrchestrationInstanceAsync(
                "UserUpdateOrchestrator", updateData);
            var response = await client.WaitForInstanceCompletionAsync(result, true, CancellationToken.None);
            if (response.RuntimeStatus == OrchestrationRuntimeStatus.Failed)
            {
                logger.LogError("Failed to update user {UserId}: {Error}", id, response.FailureDetails);
                return new StatusCodeResult(500);
            }

            if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            {
                if (response.ReadOutputAs<UserResponse>() == null)
                    return new NotFoundResult();
                return new OkObjectResult(new
                {
                    message = "User updated successfully",
                    userUpdated = response.ReadOutputAs<UserResponse>()
                });
            }

            return new StatusCodeResult(500);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user {UserId}", id);
            return new StatusCodeResult(500);
        }
    }

    [Function("DeleteUser")]
    public async Task<IActionResult> DeleteUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "users/{id}")]
        HttpRequestData req,
        string id,
        [DurableClient] DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.Admin)))
            return new UnauthorizedResult();
        try
        {
            if (!Guid.TryParse(id, out var userId))
                return new BadRequestObjectResult("Invalid user ID format");
            var result = await client.ScheduleNewOrchestrationInstanceAsync(
                "UserDeleteOrchestrator", userId);
            var response = await client.WaitForInstanceCompletionAsync(result, CancellationToken.None);
            if (response.SerializedOutput == "false")
                return new NotFoundResult();
            return new AcceptedResult("Deleted successfully", new { instanceId = result });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting user {UserId}", id);
            return new StatusCodeResult(500);
        }
    }

    [Function("RegisterUser")]
    public async Task<IActionResult> RegisterUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/register")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        // Extract registration data from request
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var registerRequest = JsonConvert.DeserializeObject<RegisterUserRequest>(requestBody);
        if (registerRequest == null)
            return new BadRequestObjectResult("Invalid registration data");
        var instance = await client.ScheduleNewOrchestrationInstanceAsync("RegisterUserOrchestrator", registerRequest);
        var response = await client.WaitForInstanceCompletionAsync(instance, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            return new OkObjectResult(response.ReadOutputAs<object>());
        return new StatusCodeResult(500);
    }

    [Function("LoginUser")]
    public async Task<IActionResult> LoginUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        // Extract login data from request
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var loginRequest = JsonConvert.DeserializeObject<LoginUserRequest>(requestBody);
        if (loginRequest == null)
            return new BadRequestObjectResult("Invalid login data");
        var instance = await client.ScheduleNewOrchestrationInstanceAsync("LoginUserOrchestrator", loginRequest);
        var response = await client.WaitForInstanceCompletionAsync(instance, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var authResponse = response.ReadOutputAs<AuthResponse>();
            if (authResponse == null || !authResponse.Success)
                return new UnauthorizedObjectResult(new { message = authResponse?.Message ?? "Unauthorized" });
            return new OkObjectResult(authResponse.Data);
        }

        return new UnauthorizedResult();
    }
}