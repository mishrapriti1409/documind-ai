using DocuMind.Application.DTOs.AI;
using DocuMind.Application.Interfaces;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;

namespace DocuMind.Application.HttpClients;

public sealed class AIServiceClient : IAIServiceClient
{
    private readonly HttpClient _http;
    private readonly ILogger<AIServiceClient> _logger;
    private static readonly JsonSerializerOptions _jsonOpts = new(JsonSerializerDefaults.Web);

    public AIServiceClient(HttpClient http, ILogger<AIServiceClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<ProcessingResultDto> ProcessDocumentAsync(
        Guid documentId, Guid organizationId, string filePath, string contentType, CancellationToken ct = default)
    {
        _logger.LogInformation("Calling AI microservice to process document {DocId}", documentId);
        var payload = new
        {
            document_id = documentId,
            organization_id = organizationId,
            file_path = filePath,
            content_type = contentType
        };
        var response = await _http.PostAsJsonAsync("/api/v1/documents/process", payload, _jsonOpts, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ProcessingResultDto>(_jsonOpts, ct);
        return result ?? throw new InvalidOperationException("Empty response from AI microservice processing");
    }

    public async Task<QueryResultDto> QueryAsync(QueryRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Calling AI microservice RAG query for conversation {ConvId}", request.ConversationId);
        var response = await _http.PostAsJsonAsync("/api/v1/query/rag", request, _jsonOpts, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<QueryResultDto>(_jsonOpts, ct);
        return result ?? throw new InvalidOperationException("Empty response from AI microservice RAG");
    }

    public async Task<SummaryResultDto> SummarizeAsync(SummarizeRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Calling AI microservice summarization for doc {DocId}", request.DocumentId);
        var response = await _http.PostAsJsonAsync("/api/v1/documents/summarize", request, _jsonOpts, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<SummaryResultDto>(_jsonOpts, ct);
        return result ?? throw new InvalidOperationException("Empty response from AI microservice summarization");
    }

    public async Task<ComparisonResultDto> CompareAsync(CompareRequestDto request, CancellationToken ct = default)
    {
        _logger.LogInformation("Calling AI microservice comparison for docs {DocA} and {DocB}", request.DocumentIdA, request.DocumentIdB);
        var response = await _http.PostAsJsonAsync("/api/v1/documents/compare", request, _jsonOpts, ct);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ComparisonResultDto>(_jsonOpts, ct);
        return result ?? throw new InvalidOperationException("Empty response from AI microservice comparison");
    }
}

