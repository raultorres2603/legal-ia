using AI_Agent.Models;
using Legal_IA.Shared.Models;

namespace Legal_IA.Functions.Orchestrators.Models;

/// <summary>
///     Input model for processing legal questions with full user context
/// </summary>
public class ProcessLegalQuestionInput
{
    public LegalQueryRequest? Request { get; set; }
    public UserContext? UserContext { get; set; }
    public UserFullContext? UserFullContext { get; set; }
}