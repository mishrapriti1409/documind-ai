namespace DocuMind.Application.Interfaces;

/// <summary>
/// Abstraction over file storage (local filesystem in dev, Azure Blob in prod).
/// </summary>
public interface IStorageService
{
    Task<string> UploadAsync(Stream content, string fileName, string contentType, Guid organizationId, Guid documentId, CancellationToken ct = default);
    Task<Stream> DownloadAsync(string storagePath, CancellationToken ct = default);
    Task DeleteAsync(string storagePath, CancellationToken ct = default);
    Task<string> GetUrlAsync(string storagePath, CancellationToken ct = default);
}

