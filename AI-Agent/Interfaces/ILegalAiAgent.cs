using AI_Agent.Models;
using Legal_IA.Shared.Models;

namespace AI_Agent.Interfaces;

public interface ILegalAiAgent
{
    Task<LegalQueryResponse> ProcessQuestionAsync(LegalQueryRequest request,
        CancellationToken cancellationToken = default);

    Task<string> GetLegalAdviceAsync(string userQuestion, CancellationToken cancellationToken = default);
    Task<bool> IsLegalQuestionAsync(string question, CancellationToken cancellationToken = default);

    // Basic methods without user context
    Task<AutonomoFormResponse> GetFormGuidanceAsync(AutonomoFormRequest request,
        CancellationToken cancellationToken = default);

    Task<string> GetQuarterlyObligationsAsync(int quarter, int year, CancellationToken cancellationToken = default);
    Task<string> GetAnnualObligationsAsync(int year, CancellationToken cancellationToken = default);

    // Enhanced methods with UserFullContext (complete user data including invoices)
    Task<LegalQueryResponse> ProcessQuestionWithFullContextAsync(LegalQueryRequest request, UserFullContext userFullContext,
        CancellationToken cancellationToken = default);

    Task<string> GetFormGuidanceWithFullContextAsync(string question, string formType, UserFullContext userFullContext,
        CancellationToken cancellationToken = default);

    Task<string> GetFormGuidanceAsync(string formType,
        CancellationToken cancellationToken = default);

    Task<string> GetQuarterlyObligationsWithFullContextAsync(string question, int quarter, int year, UserFullContext userFullContext,
        CancellationToken cancellationToken = default);

    Task<string> GetAnnualObligationsWithFullContextAsync(string question, int year, UserFullContext userFullContext,
        CancellationToken cancellationToken = default);

    Task<bool> ClassifyLegalQuestionAsync(string question, CancellationToken cancellationToken = default);
}