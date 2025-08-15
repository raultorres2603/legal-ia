using System.Text.Json.Serialization;

namespace AI_Agent.Models;

public class AutonomoFormRequest
{
    [JsonPropertyName("formType")]
    public string FormType { get; set; } = string.Empty; // e.g., "303", "130", "036"
    
    [JsonPropertyName("quarter")]
    public string Quarter { get; set; } = string.Empty; // Q1, Q2, Q3, Q4
    
    [JsonPropertyName("year")]
    public int Year { get; set; } = DateTime.Now.Year;
    
    [JsonPropertyName("regimeType")]
    public string RegimeType { get; set; } = string.Empty; // "general", "modulos", "recargo"
    
    [JsonPropertyName("activityCode")]
    public string ActivityCode { get; set; } = string.Empty; // CNAE code
    
    [JsonPropertyName("userId")]
    public string? UserId { get; set; }
    
    [JsonPropertyName("sessionId")]
    public string? SessionId { get; set; }
}