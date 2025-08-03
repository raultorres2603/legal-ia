using System.Security.Claims;
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
        public async Task<ClaimsPrincipal?> Run([OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var jwt = context.GetInput<string>();
            _logger.LogInformation("Starting JWT validation orchestration. Token received: {Token}", jwt != null ? jwt.Substring(0, Math.Min(jwt.Length, 10)) + "..." : "null");
            try
            {
                var principal = await context.CallActivityAsync<ClaimsPrincipal?>("JwtValidationActivity", jwt);
                if (principal == null)
                {
                    _logger.LogWarning("JWT validation failed. Token: {Token}", jwt != null ? jwt.Substring(0, Math.Min(jwt.Length, 10)) + "..." : "null");
                }
                else
                {
                    _logger.LogInformation("JWT validation succeeded. Claims: {Claims}", string.Join(",", principal.Claims.Select(c => c.Type + ":" + c.Value)));
                }
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during JWT validation orchestration. Token: {Token}", jwt != null ? jwt.Substring(0, Math.Min(jwt.Length, 10)) + "..." : "null");
                return null;
            }
        }
    }
}
