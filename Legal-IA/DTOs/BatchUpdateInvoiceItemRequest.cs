using System.Text.Json.Serialization;

namespace Legal_IA.DTOs;

public class BatchUpdateInvoiceItemRequest
{
    [JsonPropertyName("itemId")] public Guid ItemId { get; set; }

    [JsonPropertyName("updateRequest")] public UpdateInvoiceItemRequest UpdateRequest { get; set; } = default!;
}