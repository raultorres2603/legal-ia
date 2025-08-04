using System.Security.Claims;
using Legal_IA.DTOs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Orchestrators
{
    public class JwtValidationOrchestrator
    {
        private readonly ILogger<JwtValidationOrchestrator> _logger;

        public JwtValidationOrchestrator(ILogger<JwtValidationOrchestrator> logger)
        {
            _logger = logger;
        }

        [Function("JwtValidationOrchestrator")]
        public async Task<JwtValidationResult> Run([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var jwt = context.GetInput<string>();
            _logger.LogInformation("Starting JWT validation orchestration. Token received: {Token}", jwt != null ? jwt.Substring(0, Math.Min(jwt.Length, 10)) + "..." : "null");
            try
            {
                var result = await context.CallActivityAsync<JwtValidationResult>("JwtValidationActivity", jwt);
                if (!result.IsValid)
                {
                    _logger.LogWarning("JWT validation failed. Token: {Token}", jwt != null ? jwt.Substring(0, Math.Min(jwt.Length, 10)) + "..." : "null");
                }
                else
                {
                    _logger.LogInformation("JWT validation succeeded. Claims: {Claims}", result.Claims != null ? string.Join(",", result.Claims.Select(c => c.Key + ":" + c.Value)) : "none");
                }
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during JWT validation orchestration. Token: {Token}", jwt != null ? jwt.Substring(0, Math.Min(jwt.Length, 10)) + "..." : "null");
                return new JwtValidationResult { IsValid = false, Claims = null, UserId = null, Email = null };
            }
        }
    }
}
