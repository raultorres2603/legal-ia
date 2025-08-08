using Legal_IA.Enums;

namespace Legal_IA.Models;

public class Invoice
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string InvoiceNumber { get; set; }
    public DateTime IssueDate { get; set; }
    public string ClientName { get; set; }
    public string ClientNIF { get; set; }
    public string ClientAddress { get; set; }
    public decimal Subtotal { get; set; }
    public decimal VAT { get; set; } // IVA
    public decimal IRPF { get; set; } // Retención
    public decimal Total { get; set; }
    public string Notes { get; set; }
    public List<InvoiceItem> Items { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;
}