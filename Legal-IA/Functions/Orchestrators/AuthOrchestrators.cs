using Legal_IA.DTOs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using System.Threading.Tasks;

namespace Legal_IA.Functions.Orchestrators
{
    public static class RegisterUserOrchestrator
    {
        [Function("RegisterUserOrchestrator")]
        public static async Task<object> Run([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var registerRequest = context.GetInput<RegisterUserRequest>();
            var result = await context.CallActivityAsync<object>("RegisterUserActivity", registerRequest);
            return result;
        }
    }

    public static class LoginUserOrchestrator
    {
        [Function("LoginUserOrchestrator")]
        public static async Task<object> Run([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var loginRequest = context.GetInput<LoginUserRequest>();
            var result = await context.CallActivityAsync<object>("LoginUserActivity", loginRequest);
            return result;
        }
    }
}

