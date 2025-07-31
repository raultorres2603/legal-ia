using Legal_IA.DTOs;

namespace Legal_IA.Interfaces.Services;

/// <summary>
///     Service interface for caching operations
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
    Task RemoveAsync(string key);
    Task RemoveByPatternAsync(string pattern);
}

/// <summary>
///     Service interface for notification operations
/// </summary>
public interface INotificationService
{
    Task SendWelcomeNotificationAsync(UserResponse user);
    Task SendUserUpdateNotificationAsync(UserResponse user);
    Task SendDocumentGenerationNotificationAsync(DocumentResponse document);
    Task SendDocumentStatusChangeNotificationAsync(DocumentResponse document, string previousStatus);
}

/// <summary>
///     Service interface for AI document generation
/// </summary>
public interface IAIDocumentGeneratorService
{
    Task<string> GenerateDocumentContentAsync(DocumentResponse document, Dictionary<string, object> parameters);
}

/// <summary>
///     Service interface for file storage operations
/// </summary>
public interface IFileStorageService
{
    Task<string> SaveDocumentAsync(string content, string fileName, string contentType);
    Task<string> GetDocumentAsync(string filePath);
    Task<bool> DeleteDocumentAsync(string filePath);
    Task<string> GetDocumentUrlAsync(string filePath);
}