using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Legal_IA.DTOs;
using Legal_IA.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Activities;

public class JwtValidationActivity(IConfiguration configuration, ILogger<JwtValidationActivity> logger)
{
    private readonly JwtService _jwtService = new(configuration);

    [Function("JwtValidationActivity")]
    public JwtValidationResult Run([ActivityTrigger] string jwt)
    {
        logger.LogInformation("Starting JWT validation activity. Token received: {Token}",
            jwt != null ? jwt.Substring(0, Math.Min(jwt.Length, 10)) + "..." : "null");
        try
        {
            var principal = _jwtService.ValidateToken(jwt!);
            if (principal == null)
            {
                logger.LogWarning("JWT validation failed. Token: {Token}",
                    jwt != null ? jwt.Substring(0, Math.Min(jwt.Length, 10)) + "..." : "null");
                return new JwtValidationResult { IsValid = false };
            }

            var claims = principal.Claims.ToDictionary(c => c.Type, c => c.Value);
            logger.LogInformation("Claims extracted from JWT: {Claims}", string.Join(", ", claims.Select(c => c.Key + ":" + c.Value)));
            // Normalize role claim using general function
            var normalizedRole = NormalizeClaim(claims, "role", ClaimTypes.Role,
                "http://schemas.microsoft.com/ws/2008/06/identity/claims/role");
            logger.LogInformation("Normalized role claim value: {Role}", normalizedRole ?? "null");
            if (normalizedRole != null)
                claims["role"] = normalizedRole;
            var userId = claims.TryGetValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", out var claim) ? claim : null;
            var email = claims.TryGetValue("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress", out var claim1) ? claim1 : null;
            logger.LogInformation("JWT validation succeeded. Claims: {Claims}",
                string.Join(",", claims.Select(c => c.Key + ":" + c.Value)));
            return new JwtValidationResult { IsValid = true, UserId = userId, Email = email, Claims = claims };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception during JWT validation activity. Token: {Token}",
                jwt != null ? jwt.Substring(0, Math.Min(jwt.Length, 10)) + "..." : "null");
            return new JwtValidationResult { IsValid = false };
        }
    }

    private static string? NormalizeClaim(Dictionary<string, string> claims, params string[] possibleKeys)
    {
        // Log all claim keys for debugging
        Console.WriteLine($"Claims available for normalization: {string.Join(", ", claims.Keys)}");
        foreach (var key in possibleKeys)
        {
            if (claims.TryGetValue(key, out var claim))
                return claim;
        }
        // Try to find by suffix (e.g., ends with /role), case-insensitive and trimmed
        var match = claims.Keys.FirstOrDefault(k =>
            possibleKeys.Any(pk => k.Trim().Equals(k.Trim(), StringComparison.OrdinalIgnoreCase) ||
                                   k.Trim().EndsWith(pk.Trim(), StringComparison.OrdinalIgnoreCase)));
        return match != null ? claims[match] : null;
    }
}