using OpenAI.Chat;

namespace AI_Agent.Services;

/// <summary>
///     Service for interacting with the AI chat client to obtain legal advice, form guidance, and fiscal obligations.
/// </summary>
public class LegalAdviceService
{
    private readonly ChatClient _chatClient;

    /// <summary>
    ///     Initializes a new instance of the LegalAdviceService with the specified chat client.
    /// </summary>
    /// <param name="chatClient">The OpenAI chat client to use for completions.</param>
    public LegalAdviceService(ChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    /// <summary>
    ///     Gets legal advice from the AI based on the user's question and a system prompt.
    /// </summary>
    /// <param name="userQuestion">The user's legal question.</param>
    /// <param name="systemPrompt">The system prompt to provide context to the AI.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The AI's legal advice as a string.</returns>
    public async Task<string> GetLegalAdviceAsync(string userQuestion, string systemPrompt,
        CancellationToken cancellationToken = default)
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

    /// <summary>
    ///     Gets form guidance from the AI for a specific Spanish tax form, using a form prompt and system prompt.
    /// </summary>
    /// <param name="formPrompt">The prompt describing the form and context.</param>
    /// <param name="systemPrompt">The system prompt to provide context to the AI.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The AI's guidance as a string.</returns>
    public async Task<string> GetFormGuidanceAsync(string formPrompt, string systemPrompt,
        CancellationToken cancellationToken = default)
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

    /// <summary>
    ///     Gets a summary of quarterly fiscal obligations for freelancers in Spain from the AI.
    /// </summary>
    /// <param name="quarterPrompt">The prompt describing the quarter and context.</param>
    /// <param name="systemPrompt">The system prompt to provide context to the AI.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The AI's summary as a string.</returns>
    public async Task<string> GetQuarterlyObligationsAsync(string quarterPrompt, string systemPrompt,
        CancellationToken cancellationToken = default)
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

    /// <summary>
    ///     Gets a summary of annual fiscal obligations for freelancers in Spain from the AI.
    /// </summary>
    /// <param name="annualPrompt">The prompt describing the annual obligations and context.</param>
    /// <param name="systemPrompt">The system prompt to provide context to the AI.</param>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>The AI's summary as a string.</returns>
    public async Task<string> GetAnnualObligationsAsync(string annualPrompt, string systemPrompt,
        CancellationToken cancellationToken = default)
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