namespace AI_Agent.Models;

public class UserContext
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DNI { get; set; } = string.Empty;
    public string? CIF { get; set; }
    public string BusinessName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

    // Financial summary data
    public decimal TotalIncomeCurrentYear { get; set; }
    public decimal TotalVATCurrentYear { get; set; }
    public decimal TotalIRPFCurrentYear { get; set; }
    public decimal TotalIncomeCurrentQuarter { get; set; }
    public decimal TotalVATCurrentQuarter { get; set; }
    public decimal TotalIRPFCurrentQuarter { get; set; }
    public int InvoiceCountCurrentYear { get; set; }
    public int InvoiceCountCurrentQuarter { get; set; }

    // Activity classification
    public string? ActivityCode { get; set; } // CNAE code
    public string? TaxRegime { get; set; } // "general", "modulos", etc.
}