namespace Legal_IA.DTOs;

public class BatchUpdateInvoiceItemOrchestratorInput
{
    public Guid UserId { get; set; }
    public List<BatchUpdateInvoiceItemRequest> UpdateRequests { get; set; } = new();
}