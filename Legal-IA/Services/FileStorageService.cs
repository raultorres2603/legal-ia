using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;
using Legal_IA.Interfaces.Services;

namespace Legal_IA.Services;

/// <summary>
/// File storage service implementation using Azure Blob Storage (Azurite)
/// </summary>
public class FileStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<FileStorageService> _logger;
    private const string ContainerName = "documents";

    public FileStorageService(BlobServiceClient blobServiceClient, ILogger<FileStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public async Task<string> SaveDocumentAsync(string content, string fileName, string contentType)
    {
        try
        {
            var containerClient = await GetContainerClientAsync();
            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(content));
            await blobClient.UploadAsync(stream, overwrite: true);

            var filePath = $"{ContainerName}/{fileName}";
            _logger.LogInformation("Document saved successfully: {FilePath}", filePath);
            
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving document {FileName}", fileName);
            throw;
        }
    }

    public async Task<string> GetDocumentAsync(string filePath)
    {
        try
        {
            var containerClient = await GetContainerClientAsync();
            var fileName = Path.GetFileName(filePath);
            var blobClient = containerClient.GetBlobClient(fileName);

            var response = await blobClient.DownloadContentAsync();
            return response.Value.Content.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> DeleteDocumentAsync(string filePath)
    {
        try
        {
            var containerClient = await GetContainerClientAsync();
            var fileName = Path.GetFileName(filePath);
            var blobClient = containerClient.GetBlobClient(fileName);

            var response = await blobClient.DeleteIfExistsAsync();
            return response.Value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {FilePath}", filePath);
            return false;
        }
    }

    public async Task<string> GetDocumentUrlAsync(string filePath)
    {
        var containerClient = await GetContainerClientAsync();
        var fileName = Path.GetFileName(filePath);
        var blobClient = containerClient.GetBlobClient(fileName);

        return blobClient.Uri.ToString();
    }

    private async Task<BlobContainerClient> GetContainerClientAsync()
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync();
        return containerClient;
    }
}
