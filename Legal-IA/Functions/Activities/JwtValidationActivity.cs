using System.Security.Claims;
using Legal_IA.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;

namespace Legal_IA.Functions.Activities
{
    public class JwtValidationActivity
    {
        private readonly JwtService _jwtService;

        public JwtValidationActivity(IConfiguration configuration)
        {
            _jwtService = new JwtService(configuration);
        }

        [Function("JwtValidationActivity")]
        public ClaimsPrincipal? Run([ActivityTrigger] string jwt)
        {
            return _jwtService.ValidateToken(jwt);
        }
    }
}

