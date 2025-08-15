namespace AI_Agent.Models
{
    public class LegalQueryRequest
    {
        public string Question { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string? SessionId { get; set; }
    }
}

