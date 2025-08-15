namespace AI_Agent.Models
{
    public class AutonomoFormResponse
    {
        public string FormType { get; set; } = string.Empty;
        public string FormName { get; set; } = string.Empty;
        public string Instructions { get; set; } = string.Empty;
        public List<string> RequiredData { get; set; } = new();
        public List<string> RequiredDocuments { get; set; } = new();
        public DateTime? DueDate { get; set; }
        public string PaymentDeadline { get; set; } = string.Empty;
        public List<string> Consequences { get; set; } = new();
        public string CalculationGuidance { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

