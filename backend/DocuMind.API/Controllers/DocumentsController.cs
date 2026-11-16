using DocuMind.Application.DTOs.Documents;
using DocuMind.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocuMind.API.Controllers;

[ApiController]
[Route("api/v1/documents")]
[Authorize]
[Produces("application/json")]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documentService;

    public DocumentsController(IDocumentService documentService)
    {
        _documentService = documentService;
    }

    private Guid GetOrganizationId()
    {
        var orgClaim = User.FindFirstValue("OrganizationId");
        return string.IsNullOrEmpty(orgClaim) ? Guid.Empty : Guid.Parse(orgClaim);
    }

    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    /// <summary>Upload a business document (PDF, Word, Excel, TXT, CSV) up to 50 MB.</summary>
    [HttpPost("upload")]
    [RequestSizeLimit(52428800)]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Upload(
        [FromForm] IFormFile file,
        [FromForm] string? category,
        [FromForm] string? tags,
        CancellationToken ct)
    {
        var orgId = GetOrganizationId();
        var userId = GetUserId();

        var result = await _documentService.UploadAsync(file, category, tags, userId, orgId, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    /// <summary>Get a paginated list of organizational documents with optional category and status filters.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(DocumentListResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DocumentListResponse>> GetList(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? category = null,
        [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        var orgId = GetOrganizationId();
        var result = await _documentService.GetListAsync(orgId, page, pageSize, category, status, ct);
        return Ok(result);
    }

    /// <summary>Get document details including chunking and processing metadata.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DocumentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DocumentDto>> GetById(Guid id, CancellationToken ct)
    {
        var orgId = GetOrganizationId();
        var doc = await _documentService.GetByIdAsync(id, orgId, ct);
        if (doc == null) return NotFound(new { message = $"Document with ID {id} not found." });
        return Ok(doc);
    }

    /// <summary>Soft-delete a document from the organization's library.</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var orgId = GetOrganizationId();
        var userId = GetUserId();
        await _documentService.DeleteAsync(id, orgId, userId, ct);
        return NoContent();
    }

    /// <summary>Get real-time AI ingestion and embedding status for a document.</summary>
    [HttpGet("{id:guid}/status")]
    [ProducesResponseType(typeof(ProcessingStatusDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProcessingStatusDto>> GetStatus(Guid id, CancellationToken ct)
    {
        var orgId = GetOrganizationId();
        var status = await _documentService.GetProcessingStatusAsync(id, orgId, ct);
        return Ok(status);
    }

    /// <summary>Retry failed document ingestion and vectorization.</summary>
    [HttpPost("{id:guid}/retry")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Retry(Guid id, CancellationToken ct)
    {
        var orgId = GetOrganizationId();
        await _documentService.RetryProcessingAsync(id, orgId, ct);
        return Accepted(new { message = "Document queued for retry processing." });
    }
}

