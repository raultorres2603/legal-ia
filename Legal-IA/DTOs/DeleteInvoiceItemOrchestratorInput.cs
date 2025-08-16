using System.Text.Json.Serialization;

namespace Legal_IA.DTOs;

public class DeleteInvoiceItemOrchestratorInput
{
    [JsonPropertyName("itemId")] public Guid ItemId { get; set; }

    [JsonPropertyName("userId")] public Guid UserId { get; set; }
}