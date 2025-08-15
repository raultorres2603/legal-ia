using System.Text.Json.Serialization;

namespace Legal_IA.DTOs;

public class UpdateInvoiceItemRequest
{
    [JsonPropertyName("description")] public string? Description { get; set; }

    [JsonPropertyName("quantity")] public int? Quantity { get; set; }

    [JsonPropertyName("unitPrice")] public decimal? UnitPrice { get; set; }

    [JsonPropertyName("vat")] public decimal? VAT { get; set; }

    [JsonPropertyName("irpf")] public decimal? IRPF { get; set; }

    [JsonPropertyName("total")] public decimal? Total { get; set; }
}