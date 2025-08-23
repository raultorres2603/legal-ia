using System.Text.Json.Serialization;

namespace AI_Agent.Models;

public class LegalQueryResponse
{
    [JsonPropertyName("answer")] public string Answer { get; set; } = string.Empty;

    [JsonPropertyName("isLegalQuestion")] public bool IsLegalQuestion { get; set; }

    [JsonPropertyName("timestamp")] public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("success")] public bool Success { get; set; }

    [JsonPropertyName("errorMessage")] public string? ErrorMessage { get; set; }

    [JsonPropertyName("sessionId")] public string? SessionId { get; set; }

    [JsonPropertyName("tokensUsed")] public int TokensUsed { get; set; }

    // Additional properties to support all response types
    [JsonPropertyName("queryType")] public string QueryType { get; set; } = "general";

    [JsonPropertyName("formType")] public string? FormType { get; set; }

    [JsonPropertyName("formGuidance")] public string? FormGuidance { get; set; }

    [JsonPropertyName("obligations")] public string? Obligations { get; set; }

    [JsonPropertyName("quarter")] public int? Quarter { get; set; }

    [JsonPropertyName("year")] public int? Year { get; set; }

    [JsonPropertyName("userContextIncluded")]
    public bool UserContextIncluded { get; set; }
}