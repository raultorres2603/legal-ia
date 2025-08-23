using System.Text.Json;
using AI_Agent.Models;
using Legal_IA.Services;
using Legal_IA.Shared.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions;

public class LegalAiHttpTriggers(ILogger<LegalAiHttpTriggers> logger)
{
    [Function("ProcessLegalQuestion")]
    public async Task<IActionResult> ProcessLegalQuestion(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ai/legal/question")]
        HttpRequestData req,
        FunctionContext context,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            // Validate JWT and extract userId
            var (userId, errorResult) = await ValidateAndExtractUserId(req, client);
            if (errorResult != null) return errorResult;

            logger.LogInformation("Processing legal question request for user: {UserId}", userId);

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<LegalQueryRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (request == null) return new BadRequestObjectResult("Invalid request body");

            // Validate required fields based on query type
            if (string.IsNullOrWhiteSpace(request.Question))
                return new BadRequestObjectResult("Question field is required");

            // Validate query type specific requirements
            switch (request.QueryType?.ToLower())
            {
                case "form-guidance":
                    if (string.IsNullOrWhiteSpace(request.FormType))
                        return new BadRequestObjectResult("FormType is required for form-guidance queries");
                    break;
                case "quarterly-obligations":
                    if (!request.Quarter.HasValue || !request.Year.HasValue)
                        return new BadRequestObjectResult(
                            "Quarter and Year are required for quarterly-obligations queries");
                    if (request.Quarter < 1 || request.Quarter > 4)
                        return new BadRequestObjectResult("Quarter must be between 1 and 4");
                    break;
                case "annual-obligations":
                    if (!request.Year.HasValue)
                        return new BadRequestObjectResult("Year is required for annual-obligations queries");
                    break;
            }

            // Set the userId from JWT (remove any userId from request body)
            request.UserId = userId.ToString();

            // Start the unified orchestrator that will handle all query types with full user context
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "ProcessUnifiedLegalQuestionOrchestrator", request);

            // Wait for completion
            var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);

            if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            {
                var result = response.ReadOutputAs<LegalQueryResponse>();
                return new OkObjectResult(result);
            }

            logger.LogError("Orchestration failed with status: {Status}", response.RuntimeStatus);
            return new StatusCodeResult(500);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error processing legal question");
            return new StatusCodeResult(500);
        }
    }

    // --- Private helpers ---

    /// <summary>
    ///     Validates JWT and extracts userId. Returns (userId, errorResult).
    /// </summary>
    private static async Task<(Guid userId, IActionResult? errorResult)> ValidateAndExtractUserId(HttpRequestData req,
        DurableTaskClient client)
    {
        var jwtResult = await JwtValidationHelper.ValidateJwtAsync(req, client);
        if (!JwtValidationHelper.HasRequiredRole(jwtResult, nameof(UserRole.User)))
            return (Guid.Empty, new UnauthorizedResult());
        if (jwtResult?.UserId == null || !Guid.TryParse(jwtResult.UserId, out var userId))
            return (Guid.Empty, new BadRequestObjectResult("Invalid or missing UserId in JWT"));
        return (userId, null);
    }
}