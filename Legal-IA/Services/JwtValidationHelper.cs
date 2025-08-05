using Legal_IA.DTOs;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;

namespace Legal_IA.Services;

public static class JwtValidationHelper
{
    public static async Task<JwtValidationResult?> ValidateJwtAsync(HttpRequestData req, DurableTaskClient client)
    {
        if (!req.Headers.TryGetValues("Authorization", out var authHeaders))
            return new JwtValidationResult { IsValid = false };
        var bearer = authHeaders.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(bearer) || !bearer.StartsWith("Bearer "))
            return new JwtValidationResult { IsValid = false };
        var token = bearer.Substring("Bearer ".Length);
        var instance = await client.ScheduleNewOrchestrationInstanceAsync("JwtValidationOrchestrator", token);
        var response = await client.WaitForInstanceCompletionAsync(instance, true, CancellationToken.None);
        return response.ReadOutputAs<JwtValidationResult>()!;
    }

    public static bool HasRequiredRole(JwtValidationResult? jwtResult, params string[] requiredRoles)
    {
        if (jwtResult == null || !jwtResult.IsValid || jwtResult.Claims == null ||
            !jwtResult.Claims.TryGetValue("role", out string? userRole))
            return false;
        return requiredRoles.Any(r => string.Equals(r, userRole, StringComparison.OrdinalIgnoreCase));
    }
}