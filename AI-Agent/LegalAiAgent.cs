using System.ClientModel;
using AI_Agent.Helpers;
using AI_Agent.Interfaces;
using AI_Agent.Models;
using AI_Agent.Services;
using OpenAI;

namespace AI_Agent;

public class LegalAiAgent : ILegalAiAgent
{
    private readonly string _systemPrompt;
    private readonly LegalQuestionClassifier _questionClassifier;
    private readonly LegalAdviceService _adviceService;
    private readonly IUserDataAggregatorService _userDataAggregatorService;

    public LegalAiAgent(string apiKey, IUserDataAggregatorService userDataAggregatorService)
    {
        // Configure OpenAI client to use OpenRouter.ai
        var openRouterClient = new OpenAIClient(new ApiKeyCredential(apiKey), new OpenAIClientOptions
        {
            Endpoint = new Uri("https://openrouter.ai/api/v1")
        });

        // Use a reliable free model - Google Gemma 2 9B has excellent Spanish capabilities and is confirmed available
        var chatClient = openRouterClient.GetChatClient("google/gemma-2-9b-it:free");
        _systemPrompt = PromptBuilder.BuildSystemPrompt();
        _questionClassifier = new LegalQuestionClassifier(openRouterClient);
        _adviceService = new LegalAdviceService(chatClient);
        _userDataAggregatorService = userDataAggregatorService;
    }

    public async Task<string> GetLegalAdviceAsync(string userQuestion, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _adviceService.GetLegalAdviceAsync(userQuestion, _systemPrompt, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al procesar la consulta legal: {ex.Message}", ex);
        }
    }

    public async Task<bool> IsLegalQuestionAsync(string question, CancellationToken cancellationToken = default)
    {
        return await _questionClassifier.IsLegalQuestionAsync(question, cancellationToken);
    }

    public async Task<LegalQueryResponse> ProcessQuestionAsync(LegalQueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = new LegalQueryResponse
        {
            SessionId = request.SessionId,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                response.Answer = "Por favor, formule su consulta legal.";
                response.Success = true;
                response.IsLegalQuestion = false;
                return response;
            }

            response.IsLegalQuestion = await IsLegalQuestionAsync(request.Question, cancellationToken);

            if (!response.IsLegalQuestion)
            {
                response.Answer =
                    "Lo siento, solo puedo responder preguntas relacionadas con temas legales, fiscales y judiciales en España. " +
                    "Por favor, formule una consulta relacionada con mi área de especialización.";
                response.Success = true;
                return response;
            }

            response.Answer = await GetLegalAdviceAsync(request.Question, cancellationToken);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = $"Error al procesar la consulta: {ex.Message}";
            response.Answer = "Lo siento, ha ocurrido un error al procesar su consulta. Por favor, inténtelo de nuevo.";
        }

        return response;
    }

    public async Task<AutonomoFormResponse> GetFormGuidanceAsync(AutonomoFormRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = new AutonomoFormResponse
        {
            FormType = request.FormType,
            Success = false
        };

        try
        {
            var formPrompt = PromptBuilder.BuildFormPrompt(request);
            var content = await _adviceService.GetFormGuidanceAsync(formPrompt, _systemPrompt, cancellationToken);
            FormGuidanceParser.ParseFormGuidanceResponse(content, response);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = $"Error al obtener guía del formulario: {ex.Message}";
        }

        return response;
    }

    public async Task<string> GetQuarterlyObligationsAsync(int quarter, int year,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var quarterPrompt = PromptBuilder.BuildQuarterlyObligationsPrompt(quarter, year);
            return await _adviceService.GetQuarterlyObligationsAsync(quarterPrompt, _systemPrompt, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al obtener obligaciones trimestrales: {ex.Message}", ex);
        }
    }

    public async Task<string> GetAnnualObligationsAsync(int year, CancellationToken cancellationToken = default)
    {
        try
        {
            var annualPrompt = PromptBuilder.BuildAnnualObligationsPrompt(year);
            return await _adviceService.GetAnnualObligationsAsync(annualPrompt, _systemPrompt, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al obtener obligaciones anuales: {ex.Message}", ex);
        }
    }

    // Personalized methods with UserContext
    public async Task<LegalQueryResponse> ProcessQuestionAsync(LegalQueryRequest request, UserContext userContext,
        CancellationToken cancellationToken = default)
    {
        var response = new LegalQueryResponse
        {
            SessionId = request.SessionId,
            Timestamp = DateTime.UtcNow
        };

        try
        {
            if (string.IsNullOrWhiteSpace(request.Question))
            {
                response.Answer = $"Hola {userContext.FirstName}, por favor formule su consulta legal.";
                response.Success = true;
                response.IsLegalQuestion = false;
                return response;
            }

            response.IsLegalQuestion = await IsLegalQuestionAsync(request.Question, cancellationToken);

            if (!response.IsLegalQuestion)
            {
                response.Answer =
                    $"Hola {userContext.FirstName}, solo puedo responder preguntas relacionadas con temas legales, fiscales y judiciales en España. " +
                    "Por favor, formule una consulta relacionada con mi área de especialización.";
                response.Success = true;
                return response;
            }

            // Fetch full user context (profile, invoices, invoice items)
            var userFullContext = await _userDataAggregatorService.GetUserFullContextAsync(userContext.UserId, cancellationToken);
            var personalizedSystemPrompt = PromptBuilder.BuildSystemPrompt(userFullContext);
            response.Answer = await _adviceService.GetLegalAdviceAsync(request.Question, personalizedSystemPrompt, cancellationToken);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.ErrorMessage = $"Error al procesar la consulta: {ex.Message}";
            response.Answer =
                $"Lo siento {userContext.FirstName}, ha ocurrido un error al procesar su consulta. Por favor, inténtelo de nuevo.";
        }

        return response;
    }

    public async Task<string> GetLegalAdviceAsync(string userQuestion, UserContext userContext, CancellationToken cancellationToken = default)
    {
        try
        {
            var userFullContext = await _userDataAggregatorService.GetUserFullContextAsync(userContext.UserId, cancellationToken);
            var personalizedSystemPrompt = PromptBuilder.BuildSystemPrompt(userFullContext);
            return await _adviceService.GetLegalAdviceAsync(userQuestion, personalizedSystemPrompt, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al procesar la consulta legal personalizada: {ex.Message}", ex);
        }
    }

    public async Task<AutonomoFormResponse> GetFormGuidanceAsync(AutonomoFormRequest request, UserContext userContext, CancellationToken cancellationToken = default)
    {
        var response = new AutonomoFormResponse
        {
            FormType = request.FormType,
            Success = false
        };
        try
        {
            var userFullContext = await _userDataAggregatorService.GetUserFullContextAsync(userContext.UserId, cancellationToken);
            var personalizedSystemPrompt = PromptBuilder.BuildSystemPrompt(userFullContext);
            var formPrompt = PromptBuilder.BuildPersonalizedFormPrompt(request, userFullContext);
            var content = await _adviceService.GetFormGuidanceAsync(formPrompt, personalizedSystemPrompt, cancellationToken);
            FormGuidanceParser.ParseFormGuidanceResponse(content, response);
            response.Success = true;
        }
        catch (Exception ex)
        {
            response.ErrorMessage = $"Error al obtener guía del formulario: {ex.Message}";
        }
        return response;
    }

    public async Task<string> GetQuarterlyObligationsAsync(int quarter, int year, UserContext userContext, CancellationToken cancellationToken = default)
    {
        try
        {
            var personalizedSystemPrompt = PromptBuilder.BuildSystemPrompt(userContext);
            var quarterPrompt = PromptBuilder.BuildQuarterlyObligationsPrompt(quarter, year);
            return await _adviceService.GetQuarterlyObligationsAsync(quarterPrompt, personalizedSystemPrompt, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al obtener obligaciones trimestrales personalizadas: {ex.Message}", ex);
        }
    }

    public async Task<string> GetAnnualObligationsAsync(int year, UserContext userContext, CancellationToken cancellationToken = default)
    {
        try
        {
            var personalizedSystemPrompt = PromptBuilder.BuildSystemPrompt(userContext);
            var annualPrompt = PromptBuilder.BuildAnnualObligationsPrompt(year);
            return await _adviceService.GetAnnualObligationsAsync(annualPrompt, personalizedSystemPrompt, cancellationToken);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error al obtener obligaciones anuales personalizadas: {ex.Message}", ex);
        }
    }
}
