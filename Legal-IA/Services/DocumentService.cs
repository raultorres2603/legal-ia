using Legal_IA.DTOs;
using Legal_IA.Interfaces.Services;
using Legal_IA.Interfaces.Repositories;
using Legal_IA.Models;

namespace Legal_IA.Services;

/// <summary>
/// Document service implementation using repository pattern
/// </summary>
public class DocumentService : IDocumentService
{
    private readonly IDocumentRepository _documentRepository;
    private readonly ICacheService _cacheService;
    private readonly string _cacheKeyPrefix = "document:";
    private readonly string _userDocumentsCachePrefix = "user_docs:";

    public DocumentService(IDocumentRepository documentRepository, ICacheService cacheService)
    {
        _documentRepository = documentRepository;
        _cacheService = cacheService;
    }

    public async Task<DocumentResponse?> GetDocumentByIdAsync(Guid id)
    {
        var cacheKey = $"{_cacheKeyPrefix}{id}";
        var cachedDocument = await _cacheService.GetAsync<DocumentResponse>(cacheKey);
        
        if (cachedDocument != null)
            return cachedDocument;

        var document = await _documentRepository.GetByIdAsync(id);
        if (document == null) return null;

        var documentResponse = MapToDocumentResponse(document);
        await _cacheService.SetAsync(cacheKey, documentResponse);

        return documentResponse;
    }

    public async Task<IEnumerable<DocumentResponse>> GetDocumentsByUserIdAsync(Guid userId)
    {
        var cacheKey = $"{_userDocumentsCachePrefix}{userId}";
        var cachedDocuments = await _cacheService.GetAsync<IEnumerable<DocumentResponse>>(cacheKey);
        
        if (cachedDocuments != null)
            return cachedDocuments;

        var documents = await _documentRepository.GetByUserIdAsync(userId);
        var documentResponses = documents.Select(MapToDocumentResponse);
        
        await _cacheService.SetAsync(cacheKey, documentResponses);
        return documentResponses;
    }

    public async Task<IEnumerable<DocumentResponse>> GetDocumentsByTypeAsync(DocumentType type)
    {
        var documents = await _documentRepository.GetByTypeAsync(type);
        return documents.Select(MapToDocumentResponse);
    }

    public async Task<IEnumerable<DocumentResponse>> GetDocumentsByStatusAsync(DocumentStatus status)
    {
        var documents = await _documentRepository.GetByStatusAsync(status);
        return documents.Select(MapToDocumentResponse);
    }

    public async Task<DocumentResponse> CreateDocumentAsync(CreateDocumentRequest request)
    {
        var document = new Document
        {
            UserId = request.UserId,
            Title = request.Title,
            Type = request.Type,
            Description = request.Description,
            Content = request.Content,
            Tags = request.Tags,
            Amount = request.Amount,
            Currency = request.Currency,
            Quarter = request.Quarter,
            Year = request.Year,
            IsTemplate = request.IsTemplate,
            Status = DocumentStatus.Draft,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var createdDocument = await _documentRepository.AddAsync(document);
        
        // Invalidate user documents cache
        var userCacheKey = $"{_userDocumentsCachePrefix}{request.UserId}";
        await _cacheService.RemoveAsync(userCacheKey);

        return MapToDocumentResponse(createdDocument);
    }

    public async Task<DocumentResponse?> UpdateDocumentAsync(Guid id, UpdateDocumentRequest request)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        if (document == null) return null;

        // Update fields
        if (!string.IsNullOrEmpty(request.Title)) document.Title = request.Title;
        if (!string.IsNullOrEmpty(request.Description)) document.Description = request.Description;
        if (!string.IsNullOrEmpty(request.Content)) document.Content = request.Content;
        if (request.Status.HasValue) document.Status = request.Status.Value;
        if (!string.IsNullOrEmpty(request.Tags)) document.Tags = request.Tags;
        if (request.Amount.HasValue) document.Amount = request.Amount.Value;
        if (!string.IsNullOrEmpty(request.Currency)) document.Currency = request.Currency;
        if (request.Quarter.HasValue) document.Quarter = request.Quarter.Value;
        if (request.Year.HasValue) document.Year = request.Year.Value;

        document.UpdatedAt = DateTime.UtcNow;

        var updatedDocument = await _documentRepository.UpdateAsync(document);

        // Invalidate caches
        var cacheKey = $"{_cacheKeyPrefix}{id}";
        var userCacheKey = $"{_userDocumentsCachePrefix}{document.UserId}";
        await _cacheService.RemoveAsync(cacheKey);
        await _cacheService.RemoveAsync(userCacheKey);

        return MapToDocumentResponse(updatedDocument);
    }

    public async Task<bool> DeleteDocumentAsync(Guid id)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        if (document == null) return false;

        var userId = document.UserId;
        var result = await _documentRepository.DeleteAsync(id);

        if (result)
        {
            // Invalidate caches
            var cacheKey = $"{_cacheKeyPrefix}{id}";
            var userCacheKey = $"{_userDocumentsCachePrefix}{userId}";
            await _cacheService.RemoveAsync(cacheKey);
            await _cacheService.RemoveAsync(userCacheKey);
        }

        return result;
    }

    public async Task<DocumentResponse?> UpdateDocumentStatusAsync(Guid id, DocumentStatus status)
    {
        var document = await _documentRepository.GetByIdAsync(id);
        if (document == null) return null;

        document.Status = status;
        document.UpdatedAt = DateTime.UtcNow;

        if (status == DocumentStatus.Generated)
            document.GeneratedAt = DateTime.UtcNow;
        else if (status == DocumentStatus.Submitted)
            document.SubmittedAt = DateTime.UtcNow;

        var updatedDocument = await _documentRepository.UpdateAsync(document);

        // Invalidate caches
        var cacheKey = $"{_cacheKeyPrefix}{id}";
        var userCacheKey = $"{_userDocumentsCachePrefix}{document.UserId}";
        await _cacheService.RemoveAsync(cacheKey);
        await _cacheService.RemoveAsync(userCacheKey);

        return MapToDocumentResponse(updatedDocument);
    }

    public async Task<IEnumerable<DocumentResponse>> GetTemplatesAsync()
    {
        var templates = await _documentRepository.GetTemplatesAsync();
        return templates.Select(MapToDocumentResponse);
    }

    public async Task<IEnumerable<DocumentResponse>> SearchDocumentsAsync(string searchTerm, Guid? userId = null)
    {
        var documents = await _documentRepository.SearchAsync(searchTerm, userId);
        return documents.Select(MapToDocumentResponse);
    }

    private static DocumentResponse MapToDocumentResponse(Document document)
    {
        return new DocumentResponse
        {
            Id = document.Id,
            UserId = document.UserId,
            Title = document.Title,
            Type = document.Type,
            Description = document.Description,
            FileName = document.FileName,
            FileFormat = document.FileFormat,
            FileSize = document.FileSize,
            Status = document.Status,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt,
            GeneratedAt = document.GeneratedAt,
            SubmittedAt = document.SubmittedAt,
            IsTemplate = document.IsTemplate,
            Tags = document.Tags,
            Amount = document.Amount,
            Currency = document.Currency,
            Quarter = document.Quarter,
            Year = document.Year
        };
    }
}