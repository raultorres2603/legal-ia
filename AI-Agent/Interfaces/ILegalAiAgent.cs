using AI_Agent.Models;
using Legal_IA.Shared.Models;

namespace AI_Agent.Interfaces;

public interface ILegalAiAgent
{
    Task<LegalQueryResponse> ProcessQuestionAsync(LegalQueryRequest request,
        CancellationToken cancellationToken = default);

    Task<string> GetLegalAdviceAsync(string userQuestion, CancellationToken cancellationToken = default);
    Task<bool> IsLegalQuestionAsync(string question, CancellationToken cancellationToken = default);

    // New methods for autonomous worker guidance
    Task<AutonomoFormResponse> GetFormGuidanceAsync(AutonomoFormRequest request,
        CancellationToken cancellationToken = default);

    Task<string> GetQuarterlyObligationsAsync(int quarter, int year, CancellationToken cancellationToken = default);
    Task<string> GetAnnualObligationsAsync(int year, CancellationToken cancellationToken = default);

    // New personalized methods with UserContext
    Task<LegalQueryResponse> ProcessQuestionAsync(LegalQueryRequest request, UserContext userContext,
        CancellationToken cancellationToken = default);

    Task<string> GetLegalAdviceAsync(string userQuestion, UserContext userContext,
        CancellationToken cancellationToken = default);

    Task<AutonomoFormResponse> GetFormGuidanceAsync(AutonomoFormRequest request, UserContext userContext,
        CancellationToken cancellationToken = default);

    Task<string> GetQuarterlyObligationsAsync(int quarter, int year, UserContext userContext,
        CancellationToken cancellationToken = default);

    Task<string> GetAnnualObligationsAsync(int year, UserContext userContext,
        CancellationToken cancellationToken = default);
}