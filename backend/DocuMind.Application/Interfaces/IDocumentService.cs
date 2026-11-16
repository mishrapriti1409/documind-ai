using DocuMind.Application.DTOs.Documents;
using Microsoft.AspNetCore.Http;

namespace DocuMind.Application.Interfaces;

public interface IDocumentService
{
    Task<DocumentDto> UploadAsync(IFormFile file, string? category, string? tags, Guid userId, Guid organizationId, CancellationToken ct = default);
    Task<DocumentListResponse> GetListAsync(Guid organizationId, int page, int pageSize, string? category = null, string? statusFilter = null, CancellationToken ct = default);
    Task<DocumentDto?> GetByIdAsync(Guid documentId, Guid organizationId, CancellationToken ct = default);
    Task DeleteAsync(Guid documentId, Guid organizationId, Guid deletedByUserId, CancellationToken ct = default);
    Task<ProcessingStatusDto> GetProcessingStatusAsync(Guid documentId, Guid organizationId, CancellationToken ct = default);
    Task RetryProcessingAsync(Guid documentId, Guid organizationId, CancellationToken ct = default);
}

