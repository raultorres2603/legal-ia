using AI_Agent.Models;

namespace Legal_IA.Functions.Orchestrators.Models;

/// <summary>
///     Input model for annual obligations requests with full user context
/// </summary>
public class AnnualObligationsInput
{
    public int Year { get; set; }
    public UserFullContext? UserFullContext { get; set; }
    public string Question { get; set; } = string.Empty;
}