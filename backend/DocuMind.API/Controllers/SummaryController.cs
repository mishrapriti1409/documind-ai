using DocuMind.Application.DTOs.AI;
using DocuMind.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocuMind.API.Controllers;

[ApiController]
[Route("api/v1/documents/{id:guid}/summarize")]
[Authorize]
[Produces("application/json")]
public class SummaryController : ControllerBase
{
    private readonly IAIServiceClient _aiClient;
    private readonly IDocumentRepository _docRepo;

    public SummaryController(IAIServiceClient aiClient, IDocumentRepository docRepo)
    {
        _aiClient = aiClient;
        _docRepo = docRepo;
    }

    private Guid GetOrganizationId()
    {
        var claim = User.FindFirstValue("OrganizationId");
        return string.IsNullOrEmpty(claim) ? Guid.Empty : Guid.Parse(claim);
    }

    /// <summary>Generate executive summaries, key points, FAQs, dates, monetary terms, entities, or contractual obligations for a document.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SummaryResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SummaryResultDto>> Summarize(
        Guid id,
        [FromBody] SummarizeDocumentRequest request,
        CancellationToken ct)
    {
        var orgId = GetOrganizationId();
        var doc = await _docRepo.GetByIdAsync(id, ct);
        if (doc == null || doc.OrganizationId != orgId)
        {
            return NotFound(new { message = "Document not found." });
        }

        var result = await _aiClient.SummarizeAsync(
            new SummarizeRequestDto(id, orgId, request.SummaryType), ct);

        return Ok(result);
    }
}

public record SummarizeDocumentRequest(string SummaryType = "short");

