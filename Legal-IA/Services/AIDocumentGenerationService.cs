using System.Text;
using Legal_IA.DTOs;
using Legal_IA.Interfaces.Services;
using Legal_IA.Models;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Services;

/// <summary>
///     AI Document Generation service that creates PDF documents based on user prompts
/// </summary>
public class AiDocumentGenerationService(
    IFileStorageService fileStorageService,
    IDocumentService documentService,
    ILogger<AiDocumentGenerationService> logger) : IAIDocumentGenerationService
{
    public async Task<DocumentResponse> GenerateDocumentAsync(GenerateDocumentRequest request)
    {
        try
        {
            logger.LogInformation("Starting AI document generation for user {UserId}, type {DocumentType}",
                request.UserId, request.DocumentType);

            // Step 1: Generate document content using AI based on prompts
            var generatedContent = await GenerateContentWithAi(request);

            // Step 2: Convert content to PDF
            var pdfBytes = await ConvertToPdf(generatedContent, request.DocumentType);

            // Step 3: Generate unique filename
            var fileName = GenerateFileName(request.UserId, request.DocumentType);

            // Step 4: Upload PDF to Blob Storage
            var filePath = await fileStorageService.SaveDocumentBytesAsync(pdfBytes, fileName, "application/pdf");

            // Step 5: Create document record in database
            var createDocumentRequest = new CreateDocumentRequest
            {
                UserId = request.UserId,
                Title = request.Title ?? GenerateDefaultTitle(request.DocumentType),
                Type = request.DocumentType,
                Description = request.Description ?? "AI generated document",
                Content = generatedContent.Substring(0, Math.Min(generatedContent.Length, 5000)), // Store preview
                Tags = request.Tags ?? string.Empty,
                Amount = request.Amount,
                Currency = request.Currency ?? "EUR",
                Quarter = request.Quarter,
                Year = request.Year,
                IsTemplate = false
            };

            var documentResponse = await documentService.CreateDocumentAsync(createDocumentRequest);

            // Step 6: Update document with file information
            var updateRequest = new UpdateDocumentRequest
            {
                Status = DocumentStatus.Generated
            };

            await documentService.UpdateDocumentFileInfoAsync(documentResponse.Id, filePath, fileName,
                "application/pdf", pdfBytes.Length);

            await documentService.UpdateDocumentAsync(documentResponse.Id, updateRequest);

            logger.LogInformation("AI document generation completed successfully. Document ID: {DocumentId}",
                documentResponse.Id);

            return documentResponse;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating AI document for user {UserId}", request.UserId);
            throw;
        }
    }

    private async Task<string> GenerateContentWithAi(GenerateDocumentRequest request)
    {
        // TODO: Integrate with your preferred AI service (OpenAI, Azure OpenAI, etc.)
        // This is a placeholder implementation
        var prompt = BuildPrompt(request);

        // Simulate AI generation for now
        await Task.Delay(1000);

        return GeneratePlaceholderContent(request);
    }

    private string BuildPrompt(GenerateDocumentRequest request)
    {
        var promptBuilder = new List<string>
        {
            $"Generate a {request.DocumentType} document with the following specifications:",
            $"User prompts: {string.Join(", ", request.UserPrompts)}",
            $"Additional context: {request.AdditionalContext}"
        };

        if (request.Amount.HasValue)
            promptBuilder.Add($"Amount: {request.Amount} {request.Currency}");

        if (request.Quarter.HasValue && request.Year.HasValue)
            promptBuilder.Add($"Period: Q{request.Quarter} {request.Year}");

        return string.Join("\n", promptBuilder);
    }

    private async Task<byte[]> ConvertToPdf(string content, DocumentType documentType)
    {
        // TODO: Integrate with PDF generation library (iTextSharp, PdfSharp, etc.)
        // This is a placeholder implementation
        await Task.Delay(500);

        // For now, return a simple PDF with the content
        return Encoding.UTF8.GetBytes($"PDF Content: {content}");
    }

    private string GenerateFileName(Guid userId, DocumentType documentType)
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
        return $"{userId}_{documentType}_{timestamp}.pdf";
    }

    private string GenerateDefaultTitle(DocumentType documentType)
    {
        return $"AI Generated {documentType} - {DateTime.UtcNow:yyyy-MM-dd}";
    }

    private string GeneratePlaceholderContent(GenerateDocumentRequest request)
    {
        return $"""
                AI Generated {request.DocumentType}

                Generated on: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC

                User Prompts:
                {string.Join("\n", request.UserPrompts.Select((p, i) => $"{i + 1}. {p}"))}

                Additional Context:
                {request.AdditionalContext}

                Document Details:
                - Type: {request.DocumentType}
                - Amount: {request.Amount} {request.Currency}
                - Period: Q{request.Quarter} {request.Year}

                [This is a placeholder content. The actual implementation would use AI to generate proper document content based on the prompts and context provided.]
                """;
    }
}