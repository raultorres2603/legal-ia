using Legal_IA.DTOs;

namespace Legal_IA.Interfaces.Services;

/// <summary>
///     Interface for AI Document Generation service
/// </summary>
public interface IAIDocumentGenerationService
{
    /// <summary>
    ///     Generates a document using AI based on user prompts and context
    /// </summary>
    /// <param name="request">Document generation request with prompts and metadata</param>
    /// <returns>Generated document response with blob storage reference</returns>
    Task<DocumentResponse> GenerateDocumentAsync(GenerateDocumentRequest request);
}
