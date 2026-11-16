using DocuMind.Application.Interfaces;
using DocuMind.Application.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DocuMind.Infrastructure.Storage;

/// <summary>
/// Local filesystem implementation of IStorageService for development and on-premise environments.
/// </summary>
public sealed class LocalStorageService : IStorageService
{
    private readonly StorageOptions _options;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(IOptions<StorageOptions> options, ILogger<LocalStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    private string GetFullPath(string relativePath)
    {
        return Path.GetFullPath(Path.Combine(_options.BasePath, relativePath));
    }

    public async Task<string> UploadAsync(
        Stream content,
        string fileName,
        string contentType,
        Guid organizationId,
        Guid documentId,
        CancellationToken ct = default)
    {
        var sanitizedFileName = Path.GetFileName(fileName);
        var relativeDir = Path.Combine(organizationId.ToString(), documentId.ToString());
        var relativePath = Path.Combine(relativeDir, sanitizedFileName);
        var fullPath = GetFullPath(relativePath);

        var dir = Path.GetDirectoryName(fullPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        await using var fileStream = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        await content.CopyToAsync(fileStream, ct);

        _logger.LogInformation("File uploaded to local storage: {Path}", fullPath);
        return fullPath; // Returns absolute path for local worker & AI microservice access
    }

    public Task<Stream> DownloadAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.IsPathRooted(storagePath) ? storagePath : GetFullPath(storagePath);
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"File not found in storage at {fullPath}");
        }

        Stream stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        return Task.FromResult(stream);
    }

    public Task DeleteAsync(string storagePath, CancellationToken ct = default)
    {
        var fullPath = Path.IsPathRooted(storagePath) ? storagePath : GetFullPath(storagePath);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("Deleted file from storage: {Path}", fullPath);
        }
        return Task.CompletedTask;
    }

    public Task<string> GetUrlAsync(string storagePath, CancellationToken ct = default)
    {
        return Task.FromResult($"/api/v1/documents/raw/{Path.GetFileName(storagePath)}");
    }
}

