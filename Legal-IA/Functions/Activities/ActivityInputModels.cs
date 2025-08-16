using AI_Agent.Models;
using Legal_IA.Shared.Models;

namespace Legal_IA.Functions.Activities
{
    // Input models for activities to replace dynamic parameters
    public class ProcessLegalQuestionInput
    {
        public LegalQueryRequest Request { get; set; } = new();
        public UserContext? UserContext { get; set; }
    }

    public class FormGuidanceInput
    {
        public AutonomoFormRequest Request { get; set; } = new();
        public UserContext? UserContext { get; set; }
    }

    public class QuarterlyObligationsInput
    {
        public int Quarter { get; set; }
        public int Year { get; set; }
        public UserContext? UserContext { get; set; }
    }

    public class AnnualObligationsInput
    {
        public int Year { get; set; }
        public UserContext? UserContext { get; set; }
    }
}
