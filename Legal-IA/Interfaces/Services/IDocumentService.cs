using Legal_IA.DTOs;
using Legal_IA.Models;

namespace Legal_IA.Interfaces.Services;

/// <summary>
///     Service interface for managing document operations
/// </summary>
public interface IDocumentService
{
    Task<DocumentResponse?> GetDocumentByIdAsync(Guid id);
    Task<IEnumerable<DocumentResponse>> GetDocumentsByUserIdAsync(Guid userId);
    Task<IEnumerable<DocumentResponse>> GetDocumentsByTypeAsync(DocumentType type);
    Task<IEnumerable<DocumentResponse>> GetDocumentsByStatusAsync(DocumentStatus status);
    Task<DocumentResponse> CreateDocumentAsync(CreateDocumentRequest request);
    Task<DocumentResponse?> UpdateDocumentAsync(Guid id, UpdateDocumentRequest request);
    Task<bool> DeleteDocumentAsync(Guid id);
    Task<DocumentResponse?> UpdateDocumentStatusAsync(Guid id, DocumentStatus status);
    Task<IEnumerable<DocumentResponse>> GetTemplatesAsync();
    Task<IEnumerable<DocumentResponse>> SearchDocumentsAsync(string searchTerm, Guid? userId = null);
}