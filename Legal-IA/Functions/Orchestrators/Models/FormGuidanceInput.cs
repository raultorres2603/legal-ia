using AI_Agent.Models;

namespace Legal_IA.Functions.Orchestrators.Models;

/// <summary>
///     Input model for form guidance requests with full user context
/// </summary>
public class FormGuidanceInput
{
    public AutonomoFormRequest Request { get; set; } = new();
    public UserFullContext? UserFullContext { get; set; }
    public string Question { get; set; } = string.Empty;
    public string? FormType { get; set; }
}