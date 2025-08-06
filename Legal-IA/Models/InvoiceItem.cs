using Newtonsoft.Json;

namespace Legal_IA.Models;

public class InvoiceItem
{
    public int Id { get; set; }
    public int InvoiceId { get; set; }
    public string Description { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal VAT { get; set; } // IVA
    public decimal IRPF { get; set; } // Retención
    public decimal Total { get; set; }
    [JsonIgnore]
    public Invoice Invoice { get; set; }
}