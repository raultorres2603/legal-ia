using AI_Agent.Models;

namespace Legal_IA.Functions.Activities;

// Input models for activities to replace dynamic parameters
public class ProcessLegalQuestionInput
{
    public LegalQueryRequest Request { get; set; } = new();
    public UserFullContext? UserFullContext { get; set; }
}

public class FormGuidanceInput
{
    public AutonomoFormRequest Request { get; set; } = new();
    public UserFullContext? UserFullContext { get; set; }
    public string Question { get; set; } = string.Empty;
    public string? FormType { get; set; }
}

public class QuarterlyObligationsInput
{
    public int Quarter { get; set; }
    public int Year { get; set; }
    public UserFullContext? UserFullContext { get; set; }
    public string Question { get; set; } = string.Empty;
}

public class AnnualObligationsInput
{
    public int Year { get; set; }
    public UserFullContext? UserFullContext { get; set; }
    public string Question { get; set; } = string.Empty;
}

// New input class for building full user context
public class BuildUserFullContextInput
{
    public Guid UserId { get; set; }
    public bool IncludeUserData { get; set; } = true;
    public bool IncludeInvoiceData { get; set; } = true;
}