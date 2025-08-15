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
}