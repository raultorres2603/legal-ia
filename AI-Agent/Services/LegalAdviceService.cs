using OpenAI;
using OpenAI.Chat;
using AI_Agent.Models;
using AI_Agent.Helpers;

namespace AI_Agent.Services
{
    public class LegalAdviceService
    {
        private readonly ChatClient _chatClient;
        public LegalAdviceService(ChatClient chatClient)
        {
            _chatClient = chatClient;
        }

        public async Task<string> GetLegalAdviceAsync(string userQuestion, string systemPrompt, CancellationToken cancellationToken = default)
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userQuestion)
            };

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 1000,
                Temperature = 0.3f
            };

            var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
            return response.Value.Content[0].Text;
        }

        public async Task<string> GetFormGuidanceAsync(string formPrompt, string systemPrompt, CancellationToken cancellationToken = default)
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(formPrompt)
            };

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 1500,
                Temperature = 0.2f
            };

            var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
            return response.Value.Content[0].Text;
        }

        public async Task<string> GetQuarterlyObligationsAsync(string quarterPrompt, string systemPrompt, CancellationToken cancellationToken = default)
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(quarterPrompt)
            };

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 1200,
                Temperature = 0.2f
            };

            var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
            return response.Value.Content[0].Text;
        }

        public async Task<string> GetAnnualObligationsAsync(string annualPrompt, string systemPrompt, CancellationToken cancellationToken = default)
        {
            var messages = new List<ChatMessage>
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(annualPrompt)
            };

            var options = new ChatCompletionOptions
            {
                MaxOutputTokenCount = 1500,
                Temperature = 0.2f
            };

            var response = await _chatClient.CompleteChatAsync(messages, options, cancellationToken);
            return response.Value.Content[0].Text;
        }
    }
}

