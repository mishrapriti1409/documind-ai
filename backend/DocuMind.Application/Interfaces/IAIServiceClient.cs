using DocuMind.Application.DTOs.AI;

namespace DocuMind.Application.Interfaces;

public interface IAIServiceClient
{
    Task<ProcessingResultDto> ProcessDocumentAsync(Guid documentId, Guid organizationId, string filePath, string contentType, CancellationToken ct = default);
    Task<QueryResultDto> QueryAsync(QueryRequestDto request, CancellationToken ct = default);
    Task<SummaryResultDto> SummarizeAsync(SummarizeRequestDto request, CancellationToken ct = default);
    Task<ComparisonResultDto> CompareAsync(CompareRequestDto request, CancellationToken ct = default);
}

