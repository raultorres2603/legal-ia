namespace Legal_IA.DTOs;

public class UpdateInvoiceItemRequest
{
    public string? Description { get; set; }
    public int? Quantity { get; set; }
    public decimal? UnitPrice { get; set; }
    public decimal? VAT { get; set; }
    public decimal? IRPF { get; set; }
    public decimal? Total { get; set; }
}

