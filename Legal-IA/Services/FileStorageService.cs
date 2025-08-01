using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Legal_IA.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace Legal_IA.Services;

/// <summary>
///     File storage service implementation using Azure Blob Storage (Azurite)
/// </summary>
public class FileStorageService(BlobServiceClient blobServiceClient, ILogger<FileStorageService> logger)
    : IFileStorageService
{
    private const string ContainerName = "documents";

    public async Task<string> SaveDocumentAsync(string content, string fileName, string contentType)
    {
        try
        {
            var containerClient = await GetContainerClientAsync();
            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
            await blobClient.UploadAsync(stream, true);

            var filePath = $"{ContainerName}/{fileName}";
            logger.LogInformation("Document saved successfully: {FilePath}", filePath);

            return filePath;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving document {FileName}", fileName);
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
            logger.LogError(ex, "Error retrieving document {FilePath}", filePath);
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
            logger.LogError(ex, "Error deleting document {FilePath}", filePath);
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

    /// <summary>
    ///     Save binary document (like PDF) to blob storage
    /// </summary>
    public async Task<string> SaveDocumentBytesAsync(byte[] content, string fileName, string contentType)
    {
        try
        {
            var containerClient = await GetContainerClientAsync();
            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = new MemoryStream(content);
            await blobClient.UploadAsync(stream, true);

            // Set content type for proper handling
            await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders
            {
                ContentType = contentType
            });

            var filePath = $"{ContainerName}/{fileName}";
            logger.LogInformation("Binary document saved successfully: {FilePath}, Size: {Size} bytes",
                filePath, content.Length);

            return filePath;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error saving binary document {FileName}", fileName);
            throw;
        }
    }

    /// <summary>
    ///     Get binary document (like PDF) from blob storage
    /// </summary>
    public async Task<byte[]> GetDocumentBytesAsync(string filePath)
    {
        try
        {
            var containerClient = await GetContainerClientAsync();
            var fileName = Path.GetFileName(filePath);
            var blobClient = containerClient.GetBlobClient(fileName);

            var response = await blobClient.DownloadContentAsync();
            return response.Value.Content.ToArray();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving binary document {FilePath}", filePath);
            throw;
        }
    }

    public async Task<bool> DeleteFileAsync(string fileName)
    {
        try
        {
            var containerClient = await GetContainerClientAsync();
            var blobClient = containerClient.GetBlobClient(fileName);
            var response = await blobClient.DeleteIfExistsAsync();
            return response.Value;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting file {FileName}", fileName);
            return false;
        }
    }

    public async Task<bool> FileExistsAsync(string fileName)
    {
        try
        {
            var containerClient = await GetContainerClientAsync();
            var blobClient = containerClient.GetBlobClient(fileName);
            return await blobClient.ExistsAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if file exists {FileName}", fileName);
            return false;
        }
    }

    /// <summary>
    ///     Get document metadata from blob storage
    /// </summary>
    public async Task<(long Size, string ContentType, DateTime LastModified)> GetDocumentMetadataAsync(string filePath)
    {
        try
        {
            var containerClient = await GetContainerClientAsync();
            var fileName = Path.GetFileName(filePath);
            var blobClient = containerClient.GetBlobClient(fileName);

            var properties = await blobClient.GetPropertiesAsync();
            return (
                properties.Value.ContentLength,
                properties.Value.ContentType ?? "application/octet-stream",
                properties.Value.LastModified.DateTime
            );
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving document metadata {FilePath}", filePath);
            throw;
        }
    }

    private async Task<BlobContainerClient> GetContainerClientAsync()
    {
        var containerClient = blobServiceClient.GetBlobContainerClient(ContainerName);
        await containerClient.CreateIfNotExistsAsync();
        return containerClient;
    }
}