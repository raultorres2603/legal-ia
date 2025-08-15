namespace AI_Agent.Models
{
    public class AutonomoFormRequest
    {
        public string FormType { get; set; } = string.Empty; // e.g., "303", "130", "036"
        public string Quarter { get; set; } = string.Empty; // Q1, Q2, Q3, Q4
        public int Year { get; set; } = DateTime.Now.Year;
        public string RegimeType { get; set; } = string.Empty; // "general", "modulos", "recargo"
        public string ActivityCode { get; set; } = string.Empty; // CNAE code
        public string? UserId { get; set; }
        public string? SessionId { get; set; }
    }
}

