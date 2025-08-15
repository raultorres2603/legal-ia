using AI_Agent.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Orchestrators;

public class LegalAiOrchestrators
{
    private readonly ILogger<LegalAiOrchestrators> _logger;

    public LegalAiOrchestrators(ILogger<LegalAiOrchestrators> logger)
    {
        _logger = logger;
    }

    [Function("ProcessLegalQuestionOrchestrator")]
    public async Task<LegalQueryResponse> ProcessLegalQuestionOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var input = context.GetInput<LegalQueryRequest>();

        try
        {
            _logger.LogInformation("Starting ProcessLegalQuestionOrchestrator for question: {Question}",
                input?.Question ?? "Unknown");

            // Step 1: Build user context if userId is provided
            UserContext? userContext = null;
            if (!string.IsNullOrEmpty(input?.UserId) && Guid.TryParse(input.UserId, out var userId))
                userContext = await context.CallActivityAsync<UserContext?>("BuildUserContextActivity", userId);

            // Step 2: Process the legal question
            var response = await context.CallActivityAsync<LegalQueryResponse>("ProcessLegalQuestionActivity",
                new { Request = input, UserContext = userContext });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ProcessLegalQuestionOrchestrator");
            return new LegalQueryResponse
            {
                Success = false,
                ErrorMessage = "Error processing legal question",
                Answer = "Lo siento, ha ocurrido un error al procesar su consulta. Por favor, inténtelo de nuevo.",
                Timestamp = DateTime.UtcNow
            };
        }
    }

    [Function("GetFormGuidanceOrchestrator")]
    public async Task<AutonomoFormResponse> GetFormGuidanceOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var input = context.GetInput<AutonomoFormRequest>();

        try
        {
            _logger.LogInformation("Starting GetFormGuidanceOrchestrator for form: {FormType}",
                input?.FormType ?? "Unknown");

            // Step 1: Build user context if userId is provided
            UserContext? userContext = null;
            if (!string.IsNullOrEmpty(input?.UserId) && Guid.TryParse(input.UserId, out var userId))
                userContext = await context.CallActivityAsync<UserContext?>("BuildUserContextActivity", userId);

            // Step 2: Get form guidance
            var response = await context.CallActivityAsync<AutonomoFormResponse>("GetFormGuidanceActivity",
                new { Request = input, UserContext = userContext });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetFormGuidanceOrchestrator");
            return new AutonomoFormResponse
            {
                Success = false,
                ErrorMessage = "Error processing form guidance request",
                FormType = input?.FormType ?? "Unknown"
            };
        }
    }

    [Function("GetQuarterlyObligationsOrchestrator")]
    public async Task<string> GetQuarterlyObligationsOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var input = context.GetInput<QuarterlyObligationsRequest>();

        try
        {
            _logger.LogInformation("Starting GetQuarterlyObligationsOrchestrator for Q{Quarter} {Year}",
                input?.Quarter ?? 1, input?.Year ?? DateTime.Now.Year);

            // Step 1: Build user context if userId is provided
            UserContext? userContext = null;
            if (input?.UserId.HasValue == true)
                userContext =
                    await context.CallActivityAsync<UserContext?>("BuildUserContextActivity", input.UserId.Value);

            // Step 2: Get quarterly obligations
            var response = await context.CallActivityAsync<string>("GetQuarterlyObligationsActivity",
                new { input.Quarter, input.Year, UserContext = userContext });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetQuarterlyObligationsOrchestrator");
            return "Error al obtener obligaciones trimestrales. Por favor, inténtelo de nuevo.";
        }
    }

    [Function("GetAnnualObligationsOrchestrator")]
    public async Task<string> GetAnnualObligationsOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var input = context.GetInput<AnnualObligationsRequest>();

        try
        {
            _logger.LogInformation("Starting GetAnnualObligationsOrchestrator for year {Year}",
                input?.Year ?? DateTime.Now.Year);

            // Step 1: Build user context if userId is provided
            UserContext? userContext = null;
            if (input?.UserId.HasValue == true)
                userContext =
                    await context.CallActivityAsync<UserContext?>("BuildUserContextActivity", input.UserId.Value);

            // Step 2: Get annual obligations
            var response = await context.CallActivityAsync<string>("GetAnnualObligationsActivity",
                new { input.Year, UserContext = userContext });

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAnnualObligationsOrchestrator");
            return "Error al obtener obligaciones anuales. Por favor, inténtelo de nuevo.";
        }
    }

    [Function("ClassifyLegalQuestionOrchestrator")]
    public async Task<bool> ClassifyLegalQuestionOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var question = context.GetInput<string>();

        try
        {
            _logger.LogInformation("Starting ClassifyLegalQuestionOrchestrator for question: {Question}",
                question ?? "Unknown");

            // Call the classification activity
            var isLegalQuestion = await context.CallActivityAsync<bool>("ClassifyLegalQuestionActivity", question);

            return isLegalQuestion;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ClassifyLegalQuestionOrchestrator");
            // Return true as fallback to avoid rejecting legal questions incorrectly
            return true;
        }
    }
}

// Supporting classes for orchestrator inputs
public class QuarterlyObligationsRequest
{
    public int Quarter { get; set; }
    public int Year { get; set; }
    public Guid? UserId { get; set; }
}

public class AnnualObligationsRequest
{
    public int Year { get; set; }
    public Guid? UserId { get; set; }
}