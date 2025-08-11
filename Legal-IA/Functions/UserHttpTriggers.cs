using System.Text.Json;
using Legal_IA.DTOs;
using Legal_IA.Enums;
using Legal_IA.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions;

public class UserHttpTriggers(ILogger<UserHttpTriggers> logger)
{

    [Function("RegisterUser")]
    public async Task<IActionResult> RegisterUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/register")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        // Extract registration data from request
        var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        var registerRequest = JsonSerializer.Deserialize<RegisterUserRequest>(requestBody);
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
        var loginRequest = JsonSerializer.Deserialize<LoginUserRequest>(requestBody);
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

    [Function("VerifyUser")]
    public async Task<IActionResult> VerifyUser(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/verify")] HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
        var token = query["token"];
        if (string.IsNullOrEmpty(token))
            return new BadRequestObjectResult("Missing verification token.");

        // Call orchestrator to verify user by token
        var orchestrationId = await client.ScheduleNewOrchestrationInstanceAsync(
            "VerifyUserEmailOrchestrator", token);
        var response = await client.WaitForInstanceCompletionAsync(orchestrationId, true, CancellationToken.None);
        if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
        {
            var result = response.ReadOutputAs<AuthResponse>();
            if (result!.Success)
                return new OkObjectResult("Email verified successfully. You can now log in.");
            return new BadRequestObjectResult(result.Message ?? "Verification failed.");
        }
        return new StatusCodeResult(500);
    }
}