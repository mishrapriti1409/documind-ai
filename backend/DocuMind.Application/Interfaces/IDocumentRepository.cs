using DocuMind.Domain.Entities;
using DocuMind.Domain.Enums;

namespace DocuMind.Application.Interfaces;

public interface IDocumentRepository
{
    Task<IEnumerable<Document>> GetByOrganizationAsync(
        Guid organizationId, int page, int pageSize, string? category = null, ProcessingStatus? status = null, CancellationToken ct = default);
    Task<int> CountByOrganizationAsync(
        Guid organizationId, string? category = null, ProcessingStatus? status = null, CancellationToken ct = default);
    Task<Document?> GetWithChunksAsync(Guid documentId, CancellationToken ct = default);
    Task<IEnumerable<ProcessingJob>> GetProcessingQueueAsync(int batchSize = 10, CancellationToken ct = default);
    Task<Document?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Document> AddAsync(Document document, CancellationToken ct = default);
    Task AddJobAsync(ProcessingJob job, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid documentId, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
