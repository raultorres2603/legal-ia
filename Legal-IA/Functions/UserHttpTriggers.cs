using Legal_IA.DTOs;
using Legal_IA.Interfaces.Services;
using Legal_IA.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Legal_IA.Functions;

public class UserHttpTriggers
{
    private readonly ILogger<UserHttpTriggers> _logger;
    private readonly IUserService _userService;

    public UserHttpTriggers(ILogger<UserHttpTriggers> logger, IUserService userService)
    {
        _logger = logger;
        _userService = userService;
    }

    [Function("GetUsers")]
    public async Task<IActionResult> GetUsers(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users")]
        HttpRequestData req)
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return new OkObjectResult(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            return new StatusCodeResult(500);
        }
    }

    [Function("GetUser")]
    public async Task<IActionResult> GetUser(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id}")]
        HttpRequestData req,
        string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var userId))
                return new BadRequestObjectResult("Invalid user ID format");

            var user = await _userService.GetUserByIdAsync(userId);
            if (user == null)
                return new NotFoundResult();

            return new OkObjectResult(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user {UserId}", id);
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

            // Check if user already exists
            if (await _userService.UserExistsByEmailAsync(createRequest.Email))
                return new ConflictObjectResult("User with this email already exists");

            if (await _userService.UserExistsByDNIAsync(createRequest.DNI))
                return new ConflictObjectResult("User with this DNI already exists");

            // Start orchestration for user creation workflow
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "UserCreationOrchestrator", createRequest);

            return new AcceptedResult($"/api/orchestrations/{instanceId}", new { instanceId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user");
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
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "UserUpdateOrchestrator", updateData);

            return new AcceptedResult($"/api/orchestrations/{instanceId}", new { instanceId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", id);
            return new StatusCodeResult(500);
        }
    }

    [Function("DeleteUser")]
    public async Task<IActionResult> DeleteUser(
        [HttpTrigger(AuthorizationLevel.Function, "delete", Route = "users/{id}")]
        HttpRequestData req,
        string id)
    {
        try
        {
            if (!Guid.TryParse(id, out var userId))
                return new BadRequestObjectResult("Invalid user ID format");

            var result = await _userService.DeleteUserAsync(userId);
            if (!result)
                return new NotFoundResult();

            return new NoContentResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", id);
            return new StatusCodeResult(500);
        }
    }
}