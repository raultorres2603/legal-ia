namespace Legal_IA.Shared.Models;

public interface IUserContext
{
    Guid UserId { get; set; }
    string FirstName { get; set; }
    string LastName { get; set; }
    string Email { get; set; }
    string DNI { get; set; }
    string? CIF { get; set; }
    string BusinessName { get; set; }
    string Address { get; set; }
    string PostalCode { get; set; }
    string City { get; set; }
    string Province { get; set; }
    string Phone { get; set; }
    decimal TotalIncomeCurrentYear { get; set; }
    decimal TotalVATCurrentYear { get; set; }
    decimal TotalIRPFCurrentYear { get; set; }
    decimal TotalIncomeCurrentQuarter { get; set; }
    decimal TotalVATCurrentQuarter { get; set; }
    decimal TotalIRPFCurrentQuarter { get; set; }
    int InvoiceCountCurrentYear { get; set; }
    int InvoiceCountCurrentQuarter { get; set; }
    string? ActivityCode { get; set; } // CNAE code
    string? TaxRegime { get; set; } // "general", "modulos", etc.
}