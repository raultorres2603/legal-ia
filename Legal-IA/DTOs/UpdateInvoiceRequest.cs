using InvoiceStatus = Legal_IA.Shared.Enums.InvoiceStatus;

namespace Legal_IA.DTOs;

public class UpdateInvoiceRequest
{
    public string? InvoiceNumber { get; set; }
    public DateTime? IssueDate { get; set; }
    public string? ClientName { get; set; }
    public string? ClientNIF { get; set; }
    public string? ClientAddress { get; set; }
    public decimal? Subtotal { get; set; }
    public decimal? VAT { get; set; }
    public decimal? IRPF { get; set; }
    public decimal? Total { get; set; }
    public string? Notes { get; set; }
    public List<UpdateInvoiceItemRequest>? Items { get; set; }
    public Guid? UserId { get; set; } // Nullable for PATCH
    public InvoiceStatus? Status { get; set; }
}