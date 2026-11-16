using DocuMind.Application.DTOs.Documents;
using DocuMind.Application.Interfaces;
using DocuMind.Domain.Entities;
using DocuMind.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace DocuMind.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IStorageService _storageService;
    private readonly IDocumentRepository _docRepo;
    private readonly ILogger<DocumentService> _logger;

    private static readonly HashSet<string> _allowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".pdf", ".docx", ".doc", ".xlsx", ".xls", ".txt", ".csv"
    };

    private const long _maxFileSize = 52428800; // 50 MB

    public DocumentService(
        IStorageService storageService,
        IDocumentRepository docRepo,
        ILogger<DocumentService> logger)
    {
        _storageService = storageService;
        _docRepo = docRepo;
        _logger = logger;
    }

    public async Task<DocumentDto> UploadAsync(
        IFormFile file,
        string? category,
        string? tags,
        Guid userId,
        Guid organizationId,
        CancellationToken ct = default)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("A non-empty file must be provided.");
        }

        if (file.Length > _maxFileSize)
        {
            throw new ArgumentException($"File exceeds maximum allowed size of 50 MB. (Size: {file.Length / 1024 / 1024} MB)");
        }

        var ext = Path.GetExtension(file.FileName);
        if (!_allowedExtensions.Contains(ext))
        {
            throw new ArgumentException($"Unsupported file format '{ext}'. Allowed: PDF, DOCX, XLSX, TXT, CSV.");
        }

        var documentId = Guid.NewGuid();
        _logger.LogInformation("Uploading document {FileName} ({Size} bytes) for Org {Org}", file.FileName, file.Length, organizationId);

        // 1. Upload to storage
        await using var stream = file.OpenReadStream();
        var storagePath = await _storageService.UploadAsync(
            stream, file.FileName, file.ContentType, organizationId, documentId, ct);

        // 2. Persist Document entity
        var document = new Document
        {
            Id = documentId,
            OrganizationId = organizationId,
            UploadedByUserId = userId,
            OriginalFileName = file.FileName,
            StoragePath = storagePath,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            Category = string.IsNullOrWhiteSpace(category) ? "General" : category.Trim(),
            Tags = tags?.Trim(),
            ProcessingStatus = ProcessingStatus.Pending,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        // 3. Queue processing job
        var job = new ProcessingJob
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            Status = "Pending",
            AttemptCount = 0,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        await _docRepo.AddAsync(document, ct);
        await _docRepo.AddJobAsync(job, ct);
        await _docRepo.SaveChangesAsync(ct);

        _logger.LogInformation("Document {DocId} registered and queued for processing", documentId);

        return new DocumentDto(
            document.Id,
            document.OriginalFileName,
            document.ContentType,
            document.FileSizeBytes,
            document.Category,
            document.Tags,
            document.ProcessingStatus.ToString(),
            document.ProcessingError,
            document.TotalPages,
            document.TotalChunks,
            document.CreatedAt,
            document.ProcessedAt,
            "You"
        );
    }

    public async Task<DocumentListResponse> GetListAsync(
        Guid organizationId,
        int page,
        int pageSize,
        string? category = null,
        string? statusFilter = null,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        ProcessingStatus? status = null;
        if (!string.IsNullOrWhiteSpace(statusFilter) && Enum.TryParse<ProcessingStatus>(statusFilter, true, out var parsedStatus))
        {
            status = parsedStatus;
        }

        var total = await _docRepo.CountByOrganizationAsync(organizationId, category, status, ct);
        var docs = await _docRepo.GetByOrganizationAsync(organizationId, page, pageSize, category, status, ct);

        var dtos = docs.Select(d => new DocumentDto(
            d.Id,
            d.OriginalFileName,
            d.ContentType,
            d.FileSizeBytes,
            d.Category,
            d.Tags,
            d.ProcessingStatus.ToString(),
            d.ProcessingError,
            d.TotalPages,
            d.TotalChunks,
            d.CreatedAt,
            d.ProcessedAt,
            d.UploadedBy != null ? d.UploadedBy.FullName : "System"
        ));

        return new DocumentListResponse(dtos, total, page, pageSize);
    }

    public async Task<DocumentDto?> GetByIdAsync(Guid documentId, Guid organizationId, CancellationToken ct = default)
    {
        var doc = await _docRepo.GetWithChunksAsync(documentId, ct);
        if (doc == null || doc.OrganizationId != organizationId)
        {
            return null;
        }

        return new DocumentDto(
            doc.Id,
            doc.OriginalFileName,
            doc.ContentType,
            doc.FileSizeBytes,
            doc.Category,
            doc.Tags,
            doc.ProcessingStatus.ToString(),
            doc.ProcessingError,
            doc.TotalPages,
            doc.TotalChunks,
            doc.CreatedAt,
            doc.ProcessedAt,
            doc.UploadedBy?.FullName ?? "User"
        );
    }

    public async Task DeleteAsync(Guid documentId, Guid organizationId, Guid deletedByUserId, CancellationToken ct = default)
    {
        var doc = await _docRepo.GetByIdAsync(documentId, ct);
        if (doc == null || doc.OrganizationId != organizationId)
        {
            throw new KeyNotFoundException("Document not found.");
        }

        await _docRepo.SoftDeleteAsync(documentId, ct);
        _logger.LogInformation("Document {DocId} soft-deleted by user {UserId}", documentId, deletedByUserId);
    }

    public async Task<ProcessingStatusDto> GetProcessingStatusAsync(Guid documentId, Guid organizationId, CancellationToken ct = default)
    {
        var doc = await _docRepo.GetByIdAsync(documentId, ct);
        if (doc == null || doc.OrganizationId != organizationId)
        {
            throw new KeyNotFoundException("Document not found.");
        }

        return new ProcessingStatusDto(
            documentId,
            doc.ProcessingStatus.ToString(),
            doc.ProcessingError,
            0,
            null,
            doc.ProcessedAt
        );
    }

    public async Task RetryProcessingAsync(Guid documentId, Guid organizationId, CancellationToken ct = default)
    {
        var doc = await _docRepo.GetByIdAsync(documentId, ct);
        if (doc == null || doc.OrganizationId != organizationId)
        {
            throw new KeyNotFoundException("Document not found.");
        }

        doc.ProcessingStatus = ProcessingStatus.Pending;
        doc.ProcessingError = null;

        var job = new ProcessingJob
        {
            Id = Guid.NewGuid(),
            DocumentId = documentId,
            Status = "Pending",
            AttemptCount = 0,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _docRepo.AddJobAsync(job, ct);
        await _docRepo.SaveChangesAsync(ct);
        _logger.LogInformation("Queued retry for document {DocId}", documentId);
    }
}
