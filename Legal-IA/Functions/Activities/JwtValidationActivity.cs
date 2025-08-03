using System.Security.Claims;
using Legal_IA.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Activities
{
    public class JwtValidationActivity(IConfiguration configuration, ILogger<JwtValidationActivity> logger)
    {
        private readonly JwtService _jwtService = new(configuration);

        [Function("JwtValidationActivity")]
        public ClaimsPrincipal? Run([ActivityTrigger] string jwt)
        {
            logger.LogInformation("Starting JWT validation activity. Token received: {Token}", jwt != null ? jwt.Substring(0, Math.Min(jwt.Length, 10)) + "..." : "null");
            try
            {
                var principal = _jwtService.ValidateToken(jwt);
                if (principal == null)
                {
                    logger.LogWarning("JWT validation failed. Token: {Token}", jwt != null ? jwt.Substring(0, Math.Min(jwt.Length, 10)) + "..." : "null");
                }
                else
                {
                    logger.LogInformation("JWT validation succeeded. Claims: {Claims}", string.Join(",", principal.Claims.Select(c => c.Type + ":" + c.Value)));
                }
                return principal;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Exception during JWT validation activity. Token: {Token}", jwt != null ? jwt.Substring(0, Math.Min(jwt.Length, 10)) + "..." : "null");
                return null;
            }
        }
    }
}
