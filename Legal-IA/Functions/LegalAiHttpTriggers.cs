using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using AI_Agent.Models;
using Microsoft.DurableTask.Client;
using Legal_IA.Functions.Orchestrators;
using Microsoft.AspNetCore.Mvc;
using Legal_IA.Services;
using Legal_IA.Enums;

namespace Legal_IA.Functions
{
    public class LegalAiHttpTriggers(ILogger<LegalAiHttpTriggers> logger)
    {
        [Function("ProcessLegalQuestion")]
        public async Task<IActionResult> ProcessLegalQuestion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ai/legal/question")] HttpRequestData req,
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
                var request = System.Text.Json.JsonSerializer.Deserialize<LegalQueryRequest>(requestBody, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

                if (request == null)
                {
                    return new BadRequestObjectResult("Invalid request body");
                }

                // Set the userId from JWT (remove any userId from request body)
                request.UserId = userId.ToString();

                // Start orchestrator
                var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                    "ProcessLegalQuestionOrchestrator", request);

                // Wait for completion
                var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
                
                if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
                {
                    var result = response.ReadOutputAs<LegalQueryResponse>();
                    return new OkObjectResult(result);
                }
                else
                {
                    return new StatusCodeResult(500);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing legal question");
                return new StatusCodeResult(500);
            }
        }

        [Function("GetFormGuidance")]
        public async Task<IActionResult> GetFormGuidance(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ai/legal/form-guidance")] HttpRequestData req,
            FunctionContext context,
            [DurableClient] DurableTaskClient client)
        {
            try
            {
                // Validate JWT and extract userId
                var (userId, errorResult) = await ValidateAndExtractUserId(req, client);
                if (errorResult != null) return errorResult;

                logger.LogInformation("Processing form guidance request for user: {UserId}", userId);

                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var request = System.Text.Json.JsonSerializer.Deserialize<AutonomoFormRequest>(requestBody, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

                if (request == null)
                {
                    return new BadRequestObjectResult("Invalid request body");
                }

                // Set the userId from JWT (remove any userId from request body)
                request.UserId = userId.ToString();

                // Start orchestrator
                var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                    "GetFormGuidanceOrchestrator", request);

                // Wait for completion
                var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
                
                if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
                {
                    var result = response.ReadOutputAs<AutonomoFormResponse>();
                    return new OkObjectResult(result);
                }
                else
                {
                    return new StatusCodeResult(500);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing form guidance request");
                return new StatusCodeResult(500);
            }
        }

        [Function("GetQuarterlyObligations")]
        public async Task<IActionResult> GetQuarterlyObligations(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ai/legal/quarterly-obligations/{quarter:int}/{year:int}")] HttpRequestData req,
            FunctionContext context,
            [DurableClient] DurableTaskClient client,
            int quarter,
            int year)
        {
            try
            {
                // Validate JWT and extract userId
                var (userId, errorResult) = await ValidateAndExtractUserId(req, client);
                if (errorResult != null) return errorResult;

                logger.LogInformation("Processing quarterly obligations request for user: {UserId}, Q{Quarter} {Year}", userId, quarter, year);

                var orchestratorInput = new QuarterlyObligationsRequest
                {
                    Quarter = quarter,
                    Year = year,
                    UserId = userId // Always use authenticated user's ID from JWT
                };

                // Start orchestrator
                var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                    "GetQuarterlyObligationsOrchestrator", orchestratorInput);

                // Wait for completion
                var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
                
                if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
                {
                    var result = response.ReadOutputAs<string>();
                    return new OkObjectResult(new { obligations = result });
                }
                else
                {
                    return new StatusCodeResult(500);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing quarterly obligations request");
                return new StatusCodeResult(500);
            }
        }

        [Function("GetAnnualObligations")]
        public async Task<IActionResult> GetAnnualObligations(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "ai/legal/annual-obligations/{year:int}")] HttpRequestData req,
            FunctionContext context,
            [DurableClient] DurableTaskClient client,
            int year)
        {
            try
            {
                // Validate JWT and extract userId
                var (userId, errorResult) = await ValidateAndExtractUserId(req, client);
                if (errorResult != null) return errorResult;

                logger.LogInformation("Processing annual obligations request for user: {UserId}, Year {Year}", userId, year);

                var orchestratorInput = new AnnualObligationsRequest
                {
                    Year = year,
                    UserId = userId // Always use authenticated user's ID from JWT
                };

                // Start orchestrator
                var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                    "GetAnnualObligationsOrchestrator", orchestratorInput);

                // Wait for completion
                var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
                
                if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
                {
                    var result = response.ReadOutputAs<string>();
                    return new OkObjectResult(new { obligations = result });
                }
                else
                {
                    return new StatusCodeResult(500);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing annual obligations request");
                return new StatusCodeResult(500);
            }
        }

        [Function("ClassifyLegalQuestion")]
        public async Task<IActionResult> ClassifyLegalQuestion(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "ai/legal/classify")] HttpRequestData req,
            FunctionContext context,
            [DurableClient] DurableTaskClient client)
        {
            try
            {
                // Validate JWT and extract userId (for logging and audit purposes)
                var (userId, errorResult) = await ValidateAndExtractUserId(req, client);
                if (errorResult != null) return errorResult;

                logger.LogInformation("Processing legal question classification for user: {UserId}", userId);

                var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
                var request = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(requestBody, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                });

                if (request == null || !request.ContainsKey("question"))
                {
                    return new BadRequestObjectResult("Question field is required");
                }

                // Start orchestrator
                var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                    "ClassifyLegalQuestionOrchestrator", request["question"]);

                // Wait for completion
                var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);
                
                if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
                {
                    var isLegalQuestion = response.ReadOutputAs<bool>();
                    return new OkObjectResult(new { isLegalQuestion, question = request["question"] });
                }
                else
                {
                    return new StatusCodeResult(500);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error classifying legal question");
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
}