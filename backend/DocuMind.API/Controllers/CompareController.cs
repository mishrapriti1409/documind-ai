using DocuMind.Application.DTOs.AI;
using DocuMind.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocuMind.API.Controllers;

[ApiController]
[Route("api/v1/compare")]
[Authorize]
[Produces("application/json")]
public class CompareController : ControllerBase
{
    private readonly IAIServiceClient _aiClient;
    private readonly IDocumentRepository _docRepo;

    public CompareController(IAIServiceClient aiClient, IDocumentRepository docRepo)
    {
        _aiClient = aiClient;
        _docRepo = docRepo;
    }

    private Guid GetOrganizationId()
    {
        var claim = User.FindFirstValue("OrganizationId");
        return string.IsNullOrEmpty(claim) ? Guid.Empty : Guid.Parse(claim);
    }

    /// <summary>Semantically compare two documents or policy/contract versions, highlighting added, removed, and modified sections.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ComparisonResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ComparisonResultDto>> Compare(
        [FromBody] CompareDocumentsRequest request,
        CancellationToken ct)
    {
        var orgId = GetOrganizationId();

        var docA = await _docRepo.GetByIdAsync(request.DocumentIdA, ct);
        var docB = await _docRepo.GetByIdAsync(request.DocumentIdB, ct);

        if (docA == null || docA.OrganizationId != orgId || docB == null || docB.OrganizationId != orgId)
        {
            return BadRequest(new { message = "One or both documents do not exist or do not belong to your organization." });
        }

        var result = await _aiClient.CompareAsync(
            new CompareRequestDto(request.DocumentIdA, request.DocumentIdB, orgId), ct);

        return Ok(result);
    }
}

public record CompareDocumentsRequest(Guid DocumentIdA, Guid DocumentIdB);

