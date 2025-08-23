namespace Legal_IA.Functions.Orchestrators.Models;

/// <summary>
///     Input model for building complete user context including user data and invoices
/// </summary>
public class BuildUserFullContextInput
{
    public Guid UserId { get; set; }
    public bool IncludeUserData { get; set; } = true;
    public bool IncludeInvoiceData { get; set; } = true;
}