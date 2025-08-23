using System.Text.Json.Serialization;

namespace AI_Agent.Models;

public class LegalQueryRequest
{
    [JsonPropertyName("question")]
    public string Question { get; set; } = string.Empty;
    
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }
    
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    [JsonPropertyName("sessionId")]
    public string? SessionId { get; set; }
    
    // Additional properties to support all query types
    [JsonPropertyName("queryType")]
    public string QueryType { get; set; } = "general"; // general, form-guidance, quarterly-obligations, annual-obligations, classify
    
    [JsonPropertyName("formType")]
    public string? FormType { get; set; }
    
    [JsonPropertyName("quarter")]
    public int? Quarter { get; set; }
    
    [JsonPropertyName("year")]
    public int? Year { get; set; }
    
    [JsonPropertyName("includeUserContext")]
    public bool IncludeUserContext { get; set; } = true;
    
    [JsonPropertyName("includeInvoiceData")]
    public bool IncludeInvoiceData { get; set; } = true;
}