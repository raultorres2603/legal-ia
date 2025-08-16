using AI_Agent.Models;

namespace AI_Agent.Helpers
{
    public static class FormGuidanceParser
    {
        // Parses the AI's structured response and fills the AutonomoFormResponse object
        public static void ParseFormGuidanceResponse(string content, AutonomoFormResponse response)
        {
            // Simple parsing logic: split by section headers and assign to response fields
            // This can be improved with regex or more robust parsing as needed
            if (string.IsNullOrWhiteSpace(content)) return;

            string[] sections = new[]
            {
                "NOMBRE DEL FORMULARIO:",
                "INSTRUCCIONES GENERALES:",
                "DATOS NECESARIOS:",
                "DOCUMENTOS REQUERIDOS:",
                "FECHA LÍMITE:",
                "PLAZO DE PAGO:",
                "CONSECUENCIAS DE RETRASO:",
                "ORIENTACIÓN PARA CÁLCULOS:"
            };

            var dict = new Dictionary<string, string>();
            string currentSection = null;
            var lines = content.Split('\n');
            foreach (var line in lines)
            {
                var trimmed = line.Trim();
                if (sections.Any(s => trimmed.StartsWith(s)))
                {
                    currentSection = sections.First(s => trimmed.StartsWith(s));
                    dict[currentSection] = "";
                }
                else if (currentSection != null)
                {
                    dict[currentSection] += (dict[currentSection].Length > 0 ? "\n" : "") + trimmed;
                }
            }

            response.FormName = dict.GetValueOrDefault("NOMBRE DEL FORMULARIO:") ?? string.Empty;
            response.Instructions = dict.GetValueOrDefault("INSTRUCCIONES GENERALES:") ?? string.Empty;
            response.RequiredData = (dict.GetValueOrDefault("DATOS NECESARIOS:") ?? string.Empty)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
            response.RequiredDocuments = (dict.GetValueOrDefault("DOCUMENTOS REQUERIDOS:") ?? string.Empty)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
            response.DueDate = dict.GetValueOrDefault("FECHA LÍMITE:") ?? string.Empty;
            response.PaymentDeadline = dict.GetValueOrDefault("PLAZO DE PAGO:") ?? string.Empty;
            response.Consequences = (dict.GetValueOrDefault("CONSECUENCIAS DE RETRASO:") ?? string.Empty)
                .Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
            response.CalculationGuidance = dict.GetValueOrDefault("ORIENTACIÓN PARA CÁLCULOS:") ?? string.Empty;
        }
    }
}
