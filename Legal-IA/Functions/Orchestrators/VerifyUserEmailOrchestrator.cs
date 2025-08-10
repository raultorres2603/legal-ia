using Legal_IA.DTOs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;

namespace Legal_IA.Functions.Orchestrators;

public static class VerifyUserEmailOrchestrator
{
    [Function("VerifyUserEmailOrchestrator")]
    public static async Task<AuthResponse> Run([OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var token = context.GetInput<string>();
        var result = await context.CallActivityAsync<AuthResponse>("VerifyUserEmailActivity", token);
        return result;
    }
}

