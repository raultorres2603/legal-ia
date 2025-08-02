using Legal_IA.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Legal_IA.Functions;

public class UserHttpTriggers(ILogger<UserHttpTriggers> logger)
{
    [Function("GetUsers")]
    public async Task<IActionResult> GetUsers(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            var users = await client.ScheduleNewOrchestrationInstanceAsync(
                "UserGetAllOrchestrator", null);
            return new OkObjectResult(users);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting users");
            return new StatusCodeResult(500);
        }
    }

    [Function("GetUser")]
    public async Task<IActionResult> GetUser(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id}")]
        HttpRequestData req,
        string id,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            if (!Guid.TryParse(id, out var userId))
                return new BadRequestObjectResult("Invalid user ID format");

            var user = await client.ScheduleNewOrchestrationInstanceAsync(
                "UserGetByIdOrchestrator", userId);
            if (user == null)
                return new NotFoundResult();

            return new OkObjectResult(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting user {UserId}", id);
            return new StatusCodeResult(500);
        }
    }

    [Function("CreateUser")]
    public async Task<IActionResult> CreateUser(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "users")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var createRequest = JsonConvert.DeserializeObject<CreateUserRequest>(requestBody);

            if (createRequest == null)
                return new BadRequestObjectResult("Invalid request body");

            var result = await client.ScheduleNewOrchestrationInstanceAsync(
                "UserCreateOrchestrator", createRequest);

            return new AcceptedResult($"/api/orchestrations/{result}", new { instanceId = result });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating user");
            return new StatusCodeResult(500);
        }
    }

    [Function("UpdateUser")]
    public async Task<IActionResult> UpdateUser(
        [HttpTrigger(AuthorizationLevel.Function, "put", Route = "users/{id}")]
        HttpRequestData req,
        string id,
        [DurableClient] DurableTaskClient client)
    {
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

            return new AcceptedResult($"/api/orchestrations/{result}", new { instanceId = result });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating user {UserId}", id);
            return new StatusCodeResult(500);
        }
    }

    [Function("DeleteUser")]
    public async Task<IActionResult> DeleteUser(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "users/{id}")]
        HttpRequestData req,
        string id,
        [DurableClient] DurableTaskClient client)
    {
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
}