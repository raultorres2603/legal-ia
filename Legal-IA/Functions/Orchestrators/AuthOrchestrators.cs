using Legal_IA.DTOs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Orchestrators;

public static class RegisterUserOrchestrator
{
    [Function("RegisterUserOrchestrator")]
    public static async Task<object> Run([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("RegisterUserOrchestrator");
        logger.LogInformation("[RegisterUserOrchestrator] Orchestrator started");
        var registerRequest = context.GetInput<RegisterUserRequest>();
        var result = await context.CallActivityAsync<object>("RegisterUserActivity", registerRequest);
        logger.LogInformation("[RegisterUserOrchestrator] Orchestrator completed");
        return result;
    }
}

public static class LoginUserOrchestrator
{
    [Function("LoginUserOrchestrator")]
    public static async Task<object> Run([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var logger = context.CreateReplaySafeLogger("LoginUserOrchestrator");
        logger.LogInformation("[LoginUserOrchestrator] Orchestrator started");
        var loginRequest = context.GetInput<LoginUserRequest>();
        var result = await context.CallActivityAsync<object>("LoginUserActivity", loginRequest);
        logger.LogInformation("[LoginUserOrchestrator] Orchestrator completed");
        return result;
    }
}