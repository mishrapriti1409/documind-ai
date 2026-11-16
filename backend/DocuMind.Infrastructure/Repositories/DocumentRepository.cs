using DocuMind.Application.Interfaces;
using DocuMind.Domain.Entities;
using DocuMind.Domain.Enums;
using DocuMind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DocuMind.Infrastructure.Repositories;

public class DocumentRepository : Repository<Document>, IDocumentRepository
{
    public DocumentRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Document>> GetByOrganizationAsync(
        Guid organizationId, int page, int pageSize, string? category = null, ProcessingStatus? status = null, CancellationToken ct = default)
    {
        var query = _context.Documents
            .Include(d => d.UploadedBy)
            .Where(d => d.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(d => d.Category == category);

        if (status.HasValue)
            query = query.Where(d => d.ProcessingStatus == status.Value);

        return await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> CountByOrganizationAsync(
        Guid organizationId, string? category = null, ProcessingStatus? status = null, CancellationToken ct = default)
    {
        var query = _context.Documents.Where(d => d.OrganizationId == organizationId);

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(d => d.Category == category);

        if (status.HasValue)
            query = query.Where(d => d.ProcessingStatus == status.Value);

        return await query.CountAsync(ct);
    }

    public async Task<Document?> GetWithChunksAsync(Guid documentId, CancellationToken ct = default)
    {
        return await _context.Documents
            .Include(d => d.Chunks.OrderBy(c => c.ChunkIndex))
            .Include(d => d.UploadedBy)
            .FirstOrDefaultAsync(d => d.Id == documentId, ct);
    }

    public async Task<IEnumerable<ProcessingJob>> GetProcessingQueueAsync(int batchSize = 10, CancellationToken ct = default)
    {
        return await _context.ProcessingJobs
            .Include(j => j.Document)
            .Where(j => j.Status == "Pending" && j.AttemptCount < 3)
            .OrderBy(j => j.CreatedAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }

    public async Task AddJobAsync(ProcessingJob job, CancellationToken ct = default)
    {
        await _context.ProcessingJobs.AddAsync(job, ct);
    }

    public async Task SoftDeleteAsync(Guid documentId, CancellationToken ct = default)
    {
        var doc = await _context.Documents.FindAsync(new object[] { documentId }, ct);
        if (doc != null)
        {
            doc.DeletedAt = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(ct);
        }
    }
}
