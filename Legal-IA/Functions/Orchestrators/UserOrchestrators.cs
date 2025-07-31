using Legal_IA.DTOs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Legal_IA.Functions.Orchestrators;

/// <summary>
///     User-related orchestrator functions
/// </summary>
public class UserOrchestrators(ILogger<UserOrchestrators> logger)
{
    [Function("UserCreationOrchestrator")]
    public async Task<UserResponse> UserCreationOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var createRequest = context.GetInput<CreateUserRequest>()!;

        try
        {
            // Validate user data
            await context.CallActivityAsync("ValidateUserActivity", createRequest);

            // Create user in database
            var user = await context.CallActivityAsync<UserResponse>("CreateUserActivity", createRequest);

            // Send welcome notification
            await context.CallActivityAsync("SendWelcomeNotificationActivity", user);

            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in user creation orchestration");
            throw;
        }
    }

    [Function("UserUpdateOrchestrator")]
    public async Task<UserResponse?> UserUpdateOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var input = context.GetInput<dynamic>()!;
        var userId = Guid.Parse(input.UserId.ToString());
        var updateRequest = JsonConvert.DeserializeObject<UpdateUserRequest>(input.UpdateRequest.ToString());

        try
        {
            // Validate update data
            await context.CallActivityAsync("ValidateUserUpdateActivity", updateRequest);

            // Update user in database
            var user = await context.CallActivityAsync<UserResponse?>("UpdateUserActivity",
                new { UserId = userId, UpdateRequest = updateRequest });

            if (user != null)
                // Send update notification
                await context.CallActivityAsync("SendUserUpdateNotificationActivity", user);

            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error in user update orchestration for user {userId}");
            throw;
        }
    }
}