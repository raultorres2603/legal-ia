using AI_Agent.Models;
using Legal_IA.Functions.Orchestrators.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Functions.Orchestrators;

public class LegalAiOrchestrators(ILogger<LegalAiOrchestrators> logger)
{
    [Function("ProcessUnifiedLegalQuestionOrchestrator")]
    public async Task<LegalQueryResponse> ProcessUnifiedLegalQuestionOrchestrator(
        [OrchestrationTrigger] TaskOrchestrationContext context)
    {
        var input = context.GetInput<LegalQueryRequest>();

        try
        {
            logger.LogInformation(
                "Starting ProcessUnifiedLegalQuestionOrchestrator for question: {Question}, QueryType: {QueryType}",
                input?.Question ?? "Unknown", input?.QueryType ?? "general");

            // Step 1: Build comprehensive user context including invoices if userId is provided
            UserFullContext? userFullContext = null;
            if (!string.IsNullOrEmpty(input?.UserId) && Guid.TryParse(input.UserId, out var userId))
                if (input.IncludeUserContext || input.IncludeInvoiceData)
                    userFullContext = await context.CallActivityAsync<UserFullContext?>("BuildUserFullContextActivity",
                        new BuildUserFullContextInput
                        {
                            UserId = userId,
                            IncludeUserData = input.IncludeUserContext,
                            IncludeInvoiceData = input.IncludeInvoiceData
                        });

            // Step 2: Process based on query type
            LegalQueryResponse response;

            switch (input?.QueryType.ToLower())
            {
                case "form-guidance":
                    response = await ProcessFormGuidanceQuery(context, input, userFullContext);
                    break;
                case "quarterly-obligations":
                    response = await ProcessQuarterlyObligationsQuery(context, input, userFullContext);
                    break;
                case "annual-obligations":
                    response = await ProcessAnnualObligationsQuery(context, input, userFullContext);
                    break;
                case "classify":
                    response = await ProcessClassificationQuery(context, input);
                    break;
                default:
                    response = await ProcessGeneralLegalQuery(context, input!, userFullContext);
                    break;
            }

            response.Success = true;
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in ProcessUnifiedLegalQuestionOrchestrator");
            return new LegalQueryResponse
            {
                Success = false,
                ErrorMessage = "Error processing legal question",
                Answer = "Lo siento, ha ocurrido un error al procesar su consulta. Por favor, inténtelo de nuevo.",
                Timestamp = DateTime.UtcNow,
                QueryType = input?.QueryType ?? "general",
                SessionId = input?.SessionId
            };
        }
    }

    private async Task<LegalQueryResponse> ProcessGeneralLegalQuery(
        TaskOrchestrationContext context,
        LegalQueryRequest input,
        UserFullContext? userFullContext)
    {
        var activityInput = new ProcessLegalQuestionInput
        {
            Request = input,
            UserFullContext = userFullContext
        };

        var response =
            await context.CallActivityAsync<LegalQueryResponse>("ProcessLegalQuestionWithFullContextActivity",
                activityInput);
        response.QueryType = "general";
        response.UserContextIncluded = userFullContext != null;
        return response;
    }

    private async Task<LegalQueryResponse> ProcessFormGuidanceQuery(
        TaskOrchestrationContext context,
        LegalQueryRequest input,
        UserFullContext? userFullContext)
    {
        var activityInput = new FormGuidanceInput
        {
            Question = input.Question,
            FormType = input.FormType,
            UserFullContext = userFullContext
        };

        var guidance = await context.CallActivityAsync<string>("GetFormGuidanceWithFullContextActivity", activityInput);

        return new LegalQueryResponse
        {
            Answer = guidance,
            FormGuidance = guidance,
            FormType = input.FormType,
            QueryType = "form-guidance",
            UserContextIncluded = userFullContext != null,
            Timestamp = DateTime.UtcNow,
            SessionId = input.SessionId,
            Success = true
        };
    }

    private async Task<LegalQueryResponse> ProcessQuarterlyObligationsQuery(
        TaskOrchestrationContext context,
        LegalQueryRequest input,
        UserFullContext? userFullContext)
    {
        var activityInput = new QuarterlyObligationsInput
        {
            Quarter = input.Quarter ?? 1,
            Year = input.Year ?? DateTime.Now.Year,
            UserFullContext = userFullContext,
            Question = input.Question
        };

        var obligations =
            await context.CallActivityAsync<string>("GetQuarterlyObligationsWithFullContextActivity", activityInput);

        return new LegalQueryResponse
        {
            Answer = obligations,
            Obligations = obligations,
            Quarter = input.Quarter,
            Year = input.Year,
            QueryType = "quarterly-obligations",
            UserContextIncluded = userFullContext != null,
            Timestamp = DateTime.UtcNow,
            SessionId = input.SessionId,
            Success = true
        };
    }

    private async Task<LegalQueryResponse> ProcessAnnualObligationsQuery(
        TaskOrchestrationContext context,
        LegalQueryRequest input,
        UserFullContext? userFullContext)
    {
        var activityInput = new AnnualObligationsInput
        {
            Year = input.Year ?? DateTime.Now.Year,
            UserFullContext = userFullContext,
            Question = input.Question
        };

        var obligations =
            await context.CallActivityAsync<string>("GetAnnualObligationsWithFullContextActivity", activityInput);

        return new LegalQueryResponse
        {
            Answer = obligations,
            Obligations = obligations,
            Year = input.Year,
            QueryType = "annual-obligations",
            UserContextIncluded = userFullContext != null,
            Timestamp = DateTime.UtcNow,
            SessionId = input.SessionId,
            Success = true
        };
    }

    private async Task<LegalQueryResponse> ProcessClassificationQuery(
        TaskOrchestrationContext context,
        LegalQueryRequest input)
    {
        var isLegalQuestion = await context.CallActivityAsync<bool>("ClassifyLegalQuestionActivity", input.Question);

        return new LegalQueryResponse
        {
            Answer = isLegalQuestion ? "Esta es una pregunta legal válida." : "Esta no parece ser una pregunta legal.",
            IsLegalQuestion = isLegalQuestion,
            QueryType = "classify",
            UserContextIncluded = false,
            Timestamp = DateTime.UtcNow,
            SessionId = input.SessionId,
            Success = true
        };
    }
}