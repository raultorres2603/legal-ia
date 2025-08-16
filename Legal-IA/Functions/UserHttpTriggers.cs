using System.Web;
using Legal_IA.DTOs;
using Legal_IA.Shared.Enums;
using Legal_IA.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions;

/// <summary>
///     HTTP-triggered Azure Functions for user authentication and management.
/// </summary>
public class UserHttpTriggers(ILogger<UserHttpTriggers> logger)
{
    /// <summary>
    ///     Registers a new user.
    /// </summary>
    [Function("RegisterUser")]
    public async Task<IActionResult> RegisterUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/register")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        logger.LogInformation("[RegisterUser] Endpoint called");
        var registerRequest = await req.ReadFromJsonAsync<RegisterUserRequest>();
        if (registerRequest == null)
        {
            logger.LogWarning("[RegisterUser] Invalid registration data");
            return new BadRequestObjectResult("Invalid registration data");
        }

        var instance = await client.ScheduleNewOrchestrationInstanceAsync("RegisterUserOrchestrator", registerRequest);
        var response = await client.WaitForInstanceCompletionAsync(instance, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            logger.LogInformation("[RegisterUser] Registration completed successfully");
            return new ObjectResult(response.ReadOutputAs<object>())
            {
                StatusCode = 201 // Created
            };;
        }

        logger.LogError("[RegisterUser] Registration failed");
        return new StatusCodeResult(500);
    }

    /// <summary>
    ///     Logs in a user.
    /// </summary>
    [Function("LoginUser")]
    public async Task<IActionResult> LoginUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        logger.LogInformation("[LoginUser] Endpoint called");
        var loginRequest = await req.ReadFromJsonAsync<LoginUserRequest>();
        if (loginRequest == null)
        {
            logger.LogWarning("[LoginUser] Invalid login data");
            return new BadRequestObjectResult("Invalid login data");
        }

        var instance = await client.ScheduleNewOrchestrationInstanceAsync("LoginUserOrchestrator", loginRequest);
        var response = await client.WaitForInstanceCompletionAsync(instance, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var authResponse = response.ReadOutputAs<AuthResponse>();
            if (authResponse == null || !authResponse.Success)
            {
                logger.LogWarning("[LoginUser] Unauthorized login attempt");
                return new UnauthorizedObjectResult(new { message = authResponse?.Message ?? "Unauthorized" });
            }

            logger.LogInformation("[LoginUser] Login successful");
            return new OkObjectResult(authResponse.Data);
        }

        logger.LogWarning("[LoginUser] Unauthorized login attempt");
        return new UnauthorizedResult();
    }

    /// <summary>
    ///     Verifies a user's email by token.
    /// </summary>
    [Function("VerifyUser")]
    public async Task<IActionResult> VerifyUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/verify")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        logger.LogInformation("[VerifyUser] Endpoint called");
        var query = HttpUtility.ParseQueryString(req.Url.Query);
        var token = query["token"];
        if (string.IsNullOrEmpty(token))
        {
            logger.LogWarning("[VerifyUser] Missing verification token");
            return new BadRequestObjectResult("Missing verification token.");
        }

        var orchestrationId = await client.ScheduleNewOrchestrationInstanceAsync("VerifyUserEmailOrchestrator", token);
        var response = await client.WaitForInstanceCompletionAsync(orchestrationId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var result = response.ReadOutputAs<AuthResponse>();
            if (result!.Success)
            {
                logger.LogInformation("[VerifyUser] Email verified successfully");
                return new OkObjectResult("Email verified successfully. You can now log in.");
            }

            logger.LogWarning("[VerifyUser] Verification failed: {Message}", result.Message);
            return new BadRequestObjectResult(result.Message ?? "Verification failed.");
        }

        logger.LogError("[VerifyUser] Verification failed due to server error");
        return new StatusCodeResult(500);
    }

    /// <summary>
    ///     Updates the current user's profile.
    /// </summary>
    [Function("PatchUser")]
    public async Task<IActionResult> PatchUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "patch", Route = "user/me")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        logger.LogInformation("[PatchUser] Endpoint called");
        var (userId, errorResult) = await ValidateAndExtractUserId(req, client);
        if (errorResult != null) return errorResult;
        var updateRequest = await req.ReadFromJsonAsync<UpdateUserRequest>();
        if (updateRequest == null || updateRequest.IsActive.HasValue)
        {
            logger.LogWarning("[PatchUser] Invalid update data");
            return new BadRequestObjectResult("Invalid update data");
        }

        var orchestrationInput = new UpdateUserOrchestrationInput
        {
            UserId = userId,
            UpdateRequest = updateRequest
        };
        var instance = await client.ScheduleNewOrchestrationInstanceAsync("UserPatchOrchestrator", orchestrationInput);
        var response = await client.WaitForInstanceCompletionAsync(instance, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var userResponse = response.ReadOutputAs<UserResponse>();
            if (userResponse == null)
            {
                logger.LogWarning("[PatchUser] User not found for update");
                return new NotFoundObjectResult("User not found.");
            }

            logger.LogInformation("[PatchUser] User updated successfully");
            return new OkObjectResult(userResponse);
        }

        logger.LogError("[PatchUser] User update failed due to server error");
        return new StatusCodeResult(500);
    }

    // --- Private helpers ---

    /// <summary>
    ///     Validates JWT and extracts userId. Returns (userId, errorResult).
    /// </summary>
    private static async Task<(Guid userId, IActionResult errorResult)> ValidateAndExtractUserId(HttpRequestData req,
        DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User)) ||
            jwtResult is not { IsValid: true } || jwtResult.Claims == null || jwtResult.UserId == null ||
            !Guid.TryParse(jwtResult.UserId, out var userId))
            return (Guid.Empty, new UnauthorizedResult());
        return (userId, null);
    }
}