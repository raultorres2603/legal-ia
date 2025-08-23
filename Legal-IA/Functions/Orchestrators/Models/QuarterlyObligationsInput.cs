using AI_Agent.Models;
using Legal_IA.Shared.Models;

namespace Legal_IA.Functions.Orchestrators.Models
{
    /// <summary>
    /// Input model for quarterly obligations requests with full user context
    /// </summary>
    public class QuarterlyObligationsInput
    {
        public int Quarter { get; set; }
        public int Year { get; set; }
        public UserFullContext? UserFullContext { get; set; }
        public string Question { get; set; } = string.Empty;
    }
}
