using OpenAI;
using OpenAI.Chat;

namespace AI_Agent.Services
{
    public class LegalQuestionClassifier
    {
        private readonly OpenAIClient _openAiClient;
        public LegalQuestionClassifier(OpenAIClient openAiClient)
        {
            _openAiClient = openAiClient;
        }

        public async Task<bool> IsLegalQuestionAsync(string question, CancellationToken cancellationToken = default)
        {
            try
            {
                var classificationPrompt = @"
Clasifica si esta pregunta está relacionada con temas legales, fiscales o judiciales en España.

Responde ÚNICAMENTE con una palabra: 'SÍ' o 'NO'

Temas legales incluyen:
- IVA, IRPF, impuestos, declaraciones fiscales
- Modelos de Hacienda (303, 130, 131, etc.)
- Autónomos, obligaciones fiscales
- Derecho civil, penal, laboral, mercantil
- Contratos, obligaciones legales
- Seguridad Social

Pregunta: " + question;

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(classificationPrompt)
                };

                var chatClient = _openAiClient.GetChatClient("google/gemma-2-9b-it:free");
                var options = new ChatCompletionOptions
                {
                    MaxOutputTokenCount = 5,
                    Temperature = 0.0f
                };

                var response = await chatClient.CompleteChatAsync(messages, options, cancellationToken);
                var answer = response.Value.Content[0].Text.Trim().ToUpper();

                bool isLegal = answer.Contains("SÍ") || answer.Contains("SI") || answer.StartsWith("S");

                if (!isLegal)
                {
                    var legalKeywords = new[] { "iva", "irpf", "impuesto", "fiscal", "hacienda", "modelo", "autonomo", "autónomo", 
                                              "declaracion", "declaración", "derecho", "legal", "contrato", "obligacion", "obligación" };
                    var questionLower = question.ToLower();
                    isLegal = legalKeywords.Any(keyword => questionLower.Contains(keyword));
                }

                return isLegal;
            }
            catch
            {
                return true;
            }
        }
    }
}

