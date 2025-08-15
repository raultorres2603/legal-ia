using System.Net;
using System.Text.Json;
using System.Web;
using AI_Agent.Models;
using Legal_IA.Functions.Orchestrators;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions;

public class LegalAiHttpTriggers
{
    private readonly ILogger<LegalAiHttpTriggers> _logger;

    public LegalAiHttpTriggers(ILogger<LegalAiHttpTriggers> logger)
    {
        _logger = logger;
    }

    [Function("ProcessLegalQuestion")]
    public async Task<HttpResponseData> ProcessLegalQuestion(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ai/legal/question")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            _logger.LogInformation("Processing legal question request");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<LegalQueryRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (request == null)
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");

            // Start orchestrator
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "ProcessLegalQuestionOrchestrator", request);

            // Wait for completion
            var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);

            if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            {
                var result = response.ReadOutputAs<LegalQueryResponse>();
                return await CreateSuccessResponse(req, result);
            }

            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError,
                "Error processing legal question - orchestration failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing legal question");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError,
                "Error processing legal question");
        }
    }

    [Function("GetFormGuidance")]
    public async Task<HttpResponseData> GetFormGuidance(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ai/legal/form-guidance")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            _logger.LogInformation("Processing form guidance request");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<AutonomoFormRequest>(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (request == null)
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Invalid request body");

            // Start orchestrator
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "GetFormGuidanceOrchestrator", request);

            // Wait for completion
            var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);

            if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            {
                var result = response.ReadOutputAs<AutonomoFormResponse>();
                return await CreateSuccessResponse(req, result);
            }

            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError,
                "Error processing form guidance request - orchestration failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing form guidance request");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError,
                "Error processing form guidance request");
        }
    }

    [Function("GetQuarterlyObligations")]
    public async Task<HttpResponseData> GetQuarterlyObligations(
        [HttpTrigger(AuthorizationLevel.Function, "get",
            Route = "ai/legal/quarterly-obligations/{quarter:int}/{year:int}")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            _logger.LogInformation("Processing quarterly obligations request");

            // Extract route parameters
            var quarter = int.Parse(req.FunctionContext.BindingContext.BindingData["quarter"]?.ToString() ?? "1");
            var year = int.Parse(req.FunctionContext.BindingContext.BindingData["year"]?.ToString() ??
                                 DateTime.Now.Year.ToString());

            // Check for userId in query parameters
            var query = HttpUtility.ParseQueryString(req.Url.Query);
            var userIdParam = query["userId"];

            var orchestratorInput = new QuarterlyObligationsRequest
            {
                Quarter = quarter,
                Year = year,
                UserId = !string.IsNullOrEmpty(userIdParam) && Guid.TryParse(userIdParam, out var userId)
                    ? userId
                    : null
            };

            // Start orchestrator
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "GetQuarterlyObligationsOrchestrator", orchestratorInput);

            // Wait for completion
            var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);

            if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            {
                var result = response.ReadOutputAs<string>();
                return await CreateSuccessResponse(req, new { obligations = result });
            }

            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError,
                "Error processing quarterly obligations request - orchestration failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing quarterly obligations request");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError,
                "Error processing quarterly obligations request");
        }
    }

    [Function("GetAnnualObligations")]
    public async Task<HttpResponseData> GetAnnualObligations(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "ai/legal/annual-obligations/{year:int}")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            _logger.LogInformation("Processing annual obligations request");

            // Extract route parameters
            var year = int.Parse(req.FunctionContext.BindingContext.BindingData["year"]?.ToString() ??
                                 DateTime.Now.Year.ToString());

            // Check for userId in query parameters
            var query = HttpUtility.ParseQueryString(req.Url.Query);
            var userIdParam = query["userId"];

            var orchestratorInput = new AnnualObligationsRequest
            {
                Year = year,
                UserId = !string.IsNullOrEmpty(userIdParam) && Guid.TryParse(userIdParam, out var userId)
                    ? userId
                    : null
            };

            // Start orchestrator
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "GetAnnualObligationsOrchestrator", orchestratorInput);

            // Wait for completion
            var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);

            if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            {
                var result = response.ReadOutputAs<string>();
                return await CreateSuccessResponse(req, new { obligations = result });
            }

            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError,
                "Error processing annual obligations request - orchestration failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing annual obligations request");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError,
                "Error processing annual obligations request");
        }
    }

    [Function("ClassifyLegalQuestion")]
    public async Task<HttpResponseData> ClassifyLegalQuestion(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "ai/legal/classify")]
        HttpRequestData req,
        [DurableClient] DurableTaskClient client)
    {
        try
        {
            _logger.LogInformation("Processing legal question classification");

            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var request = JsonSerializer.Deserialize<Dictionary<string, string>>(requestBody, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            if (request == null || !request.ContainsKey("question"))
                return await CreateErrorResponse(req, HttpStatusCode.BadRequest, "Question field is required");

            // Create and start a simple orchestrator for classification
            var instanceId = await client.ScheduleNewOrchestrationInstanceAsync(
                "ClassifyLegalQuestionOrchestrator", request["question"]);

            // Wait for completion
            var response = await client.WaitForInstanceCompletionAsync(instanceId, true, CancellationToken.None);

            if (response.RuntimeStatus == OrchestrationRuntimeStatus.Completed)
            {
                var isLegalQuestion = response.ReadOutputAs<bool>();
                return await CreateSuccessResponse(req, new { isLegalQuestion, question = request["question"] });
            }

            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError,
                "Error classifying legal question - orchestration failed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying legal question");
            return await CreateErrorResponse(req, HttpStatusCode.InternalServerError,
                "Error classifying legal question");
        }
    }

    private async Task<HttpResponseData> CreateSuccessResponse(HttpRequestData req, object data)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");

        var json = JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        });

        await response.WriteStringAsync(json);
        return response;
    }

    private async Task<HttpResponseData> CreateErrorResponse(HttpRequestData req, HttpStatusCode statusCode,
        string message)
    {
        var response = req.CreateResponse(statusCode);
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");

        var errorResponse = new { error = message, timestamp = DateTime.UtcNow };
        var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await response.WriteStringAsync(json);
        return response;
    }
}