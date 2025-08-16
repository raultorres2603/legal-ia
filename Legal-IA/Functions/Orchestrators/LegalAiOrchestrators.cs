using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.Extensions.Logging;
using AI_Agent.Models;
using Legal_IA.Functions.Activities;
using Legal_IA.Shared.Models;

namespace Legal_IA.Functions.Orchestrators
{
    public class LegalAiOrchestrators(ILogger<LegalAiOrchestrators> logger)
    {
        [Function("ProcessLegalQuestionOrchestrator")]
        public async Task<LegalQueryResponse> ProcessLegalQuestionOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)
        {
            var input = context.GetInput<LegalQueryRequest>();
            
            try
            {
                logger.LogInformation("Starting ProcessLegalQuestionOrchestrator for question: {Question}", input?.Question ?? "Unknown");

                // Step 1: Build user context if userId is provided
                UserContext? userContext = null;
                if (!string.IsNullOrEmpty(input?.UserId) && Guid.TryParse(input.UserId, out var userId))
                {
                    userContext = await context.CallActivityAsync<UserContext?>("BuildUserContextActivity", userId);
                }

                // Step 2: Process the legal question using strongly-typed input
                var activityInput = new ProcessLegalQuestionInput
                {
                    Request = input,
                    UserContext = userContext
                };

                var response = await context.CallActivityAsync<LegalQueryResponse>("ProcessLegalQuestionActivity", activityInput);

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ProcessLegalQuestionOrchestrator");
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
                logger.LogInformation("Starting GetFormGuidanceOrchestrator for form: {FormType}", input?.FormType ?? "Unknown");

                // Step 1: Build user context if userId is provided
                UserContext? userContext = null;
                if (!string.IsNullOrEmpty(input?.UserId) && Guid.TryParse(input.UserId, out var userId))
                {
                    userContext = await context.CallActivityAsync<UserContext?>("BuildUserContextActivity", userId);
                }

                // Step 2: Get form guidance using strongly-typed input
                var activityInput = new FormGuidanceInput
                {
                    Request = input,
                    UserContext = userContext
                };

                var response = await context.CallActivityAsync<AutonomoFormResponse>("GetFormGuidanceActivity", activityInput);

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetFormGuidanceOrchestrator");
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
                logger.LogInformation("Starting GetQuarterlyObligationsOrchestrator for Q{Quarter} {Year}", 
                    input?.Quarter ?? 1, input?.Year ?? DateTime.Now.Year);

                // Step 1: Build user context if userId is provided
                UserContext? userContext = null;
                if (input?.UserId.HasValue == true)
                {
                    userContext = await context.CallActivityAsync<UserContext?>("BuildUserContextActivity", input.UserId.Value);
                }

                // Step 2: Get quarterly obligations using strongly-typed input
                var activityInput = new QuarterlyObligationsInput
                {
                    Quarter = input.Quarter,
                    Year = input.Year,
                    UserContext = userContext
                };

                var response = await context.CallActivityAsync<string>("GetQuarterlyObligationsActivity", activityInput);

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetQuarterlyObligationsOrchestrator");
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
                logger.LogInformation("Starting GetAnnualObligationsOrchestrator for year {Year}", input?.Year ?? DateTime.Now.Year);

                // Step 1: Build user context if userId is provided
                UserContext? userContext = null;
                if (input?.UserId.HasValue == true)
                {
                    userContext = await context.CallActivityAsync<UserContext?>("BuildUserContextActivity", input.UserId.Value);
                }

                // Step 2: Get annual obligations using strongly-typed input
                var activityInput = new AnnualObligationsInput
                {
                    Year = input.Year,
                    UserContext = userContext
                };

                var response = await context.CallActivityAsync<string>("GetAnnualObligationsActivity", activityInput);

                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in GetAnnualObligationsOrchestrator");
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
                logger.LogInformation("Starting ClassifyLegalQuestionOrchestrator for question: {Question}", question ?? "Unknown");

                // Call the classification activity
                var isLegalQuestion = await context.CallActivityAsync<bool>("ClassifyLegalQuestionActivity", question);

                return isLegalQuestion;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in ClassifyLegalQuestionOrchestrator");
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
}