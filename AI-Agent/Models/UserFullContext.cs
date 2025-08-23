using Legal_IA.Shared.Models;

namespace AI_Agent.Models;

public class UserFullContext
{
    public UserContext UserContext { get; set; }
    public List<Invoice> Invoices { get; set; } = new();
    public List<InvoiceItem> InvoiceItems { get; set; } = new();
}