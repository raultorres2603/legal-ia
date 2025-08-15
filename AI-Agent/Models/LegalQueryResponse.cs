namespace AI_Agent.Models;

public class LegalQueryResponse
{
    public string Answer { get; set; } = string.Empty;
    public bool IsLegalQuestion { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string? SessionId { get; set; }
    public int TokensUsed { get; set; }
}