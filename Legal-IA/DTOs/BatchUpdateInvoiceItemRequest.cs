namespace Legal_IA.DTOs;

public class BatchUpdateInvoiceItemRequest
{
    public Guid ItemId { get; set; }
    public UpdateInvoiceItemRequest UpdateRequest { get; set; } = default!;
}

