using System.Text.Json.Serialization;

namespace DocuMind.Application.DTOs.AI;

public record ProcessingResultDto(
    [property: JsonPropertyName("document_id")] Guid DocumentId,
    [property: JsonPropertyName("chunks_created")] int ChunksCreated,
    [property: JsonPropertyName("pages_processed")] int PagesProcessed,
    [property: JsonPropertyName("status")] string Status
);

public record QueryRequestDto(
    [property: JsonPropertyName("question")] string Question,
    [property: JsonPropertyName("organization_id")] Guid OrganizationId,
    [property: JsonPropertyName("document_ids")] List<Guid>? DocumentIds,
    [property: JsonPropertyName("user_id")] Guid UserId,
    [property: JsonPropertyName("conversation_id")] Guid ConversationId,
    [property: JsonPropertyName("conversation_history")] List<Dictionary<string, string>>? ConversationHistory = null
);

public record SourceReferenceAiDto(
    [property: JsonPropertyName("chunk_id")] Guid ChunkId,
    [property: JsonPropertyName("document_id")] Guid DocumentId,
    [property: JsonPropertyName("page_number")] int PageNumber,
    [property: JsonPropertyName("excerpt")] string Excerpt,
    [property: JsonPropertyName("relevance_score")] float RelevanceScore
);

public record QueryResultDto(
    [property: JsonPropertyName("answer")] string Answer,
    [property: JsonPropertyName("sources")] List<SourceReferenceAiDto> Sources,
    [property: JsonPropertyName("tokens_used")] int TokensUsed,
    [property: JsonPropertyName("cost_estimate")] decimal CostEstimate,
    [property: JsonPropertyName("insufficient_evidence")] bool InsufficientEvidence
);

public record SummarizeRequestDto(
    [property: JsonPropertyName("document_id")] Guid DocumentId,
    [property: JsonPropertyName("organization_id")] Guid OrganizationId,
    [property: JsonPropertyName("summary_type")] string SummaryType
);

public record SummaryResultDto(
    [property: JsonPropertyName("document_id")] Guid DocumentId,
    [property: JsonPropertyName("summary_type")] string SummaryType,
    [property: JsonPropertyName("content")] object Content,
    [property: JsonPropertyName("tokens_used")] int TokensUsed
);

public record CompareRequestDto(
    [property: JsonPropertyName("document_id_a")] Guid DocumentIdA,
    [property: JsonPropertyName("document_id_b")] Guid DocumentIdB,
    [property: JsonPropertyName("organization_id")] Guid OrganizationId
);

public record ComparisonSectionDto(
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("page_references")] List<int> PageReferences
);

public record ComparisonResultDto(
    [property: JsonPropertyName("document_id_a")] Guid DocumentIdA,
    [property: JsonPropertyName("document_id_b")] Guid DocumentIdB,
    [property: JsonPropertyName("added_sections")] List<ComparisonSectionDto> AddedSections,
    [property: JsonPropertyName("removed_sections")] List<ComparisonSectionDto> RemovedSections,
    [property: JsonPropertyName("modified_sections")] List<ComparisonSectionDto> ModifiedSections,
    [property: JsonPropertyName("ai_summary")] string AiSummary,
    [property: JsonPropertyName("tokens_used")] int TokensUsed
);

