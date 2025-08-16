using InvoiceStatus = Legal_IA.Shared.Enums.InvoiceStatus;

namespace Legal_IA.DTOs;

public class CreateInvoiceRequest
{
    public string InvoiceNumber { get; set; }
    public DateTime IssueDate { get; set; }
    public string ClientName { get; set; }
    public string ClientNIF { get; set; }
    public string ClientAddress { get; set; }
    public decimal Subtotal { get; set; }
    public decimal VAT { get; set; }
    public decimal IRPF { get; set; }
    public decimal Total { get; set; }
    public string Notes { get; set; }
    public List<CreateInvoiceItemRequest> Items { get; set; }
    public Guid UserId { get; set; } // Added for consistency with model
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
}

public class CreateInvoiceItemRequest
{
    public string Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal VAT { get; set; }
    public decimal IRPF { get; set; }
    public decimal Total { get; set; }
}