using DocuMind.Application.DTOs.Chat;
using DocuMind.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace DocuMind.API.Controllers;

[ApiController]
[Route("api/v1/conversations")]
[Authorize]
[Produces("application/json")]
public class ConversationsController : ControllerBase
{
    private readonly IChatService _chatService;

    public ConversationsController(IChatService chatService)
    {
        _chatService = chatService;
    }

    private Guid GetOrganizationId()
    {
        var claim = User.FindFirstValue("OrganizationId");
        return string.IsNullOrEmpty(claim) ? Guid.Empty : Guid.Parse(claim);
    }

    private Guid GetUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    /// <summary>Create a new chat conversation thread with an optional document scope.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateConversation([FromBody] CreateConversationRequest request, CancellationToken ct)
    {
        var userId = GetUserId();
        var orgId = GetOrganizationId();
        var conv = await _chatService.CreateConversationAsync(request, userId, orgId, ct);
        return StatusCode(StatusCodes.Status201Created, conv);
    }

    /// <summary>Retrieve all conversation threads for the current authenticated user.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ConversationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ConversationDto>>> GetConversations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        var conversations = await _chatService.GetConversationsAsync(userId, page, pageSize, ct);
        return Ok(conversations);
    }

    /// <summary>Get a specific conversation thread by ID.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ConversationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ConversationDto>> GetConversation(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var conv = await _chatService.GetConversationAsync(id, userId, ct);
        if (conv == null) return NotFound(new { message = "Conversation not found." });
        return Ok(conv);
    }

    /// <summary>Send a question to the AI assistant using RAG across authorized documents.</summary>
    [HttpPost("{id:guid}/messages")]
    [EnableRateLimiting("chat")]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChatResponse>> SendMessage(
        Guid id,
        [FromBody] SendMessageRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        var orgId = GetOrganizationId();
        var response = await _chatService.SendMessageAsync(id, request, userId, orgId, ct);
        return Ok(response);
    }

    /// <summary>Get the complete message history with verifiable source citations for a conversation.</summary>
    [HttpGet("{id:guid}/messages")]
    [ProducesResponseType(typeof(IEnumerable<MessageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessages(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        var messages = await _chatService.GetMessagesAsync(id, userId, ct);
        return Ok(messages);
    }

    /// <summary>Submit user rating and qualitative feedback for an AI response.</summary>
    [HttpPost("{id:guid}/messages/{messageId:guid}/feedback")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SubmitFeedback(
        Guid id,
        Guid messageId,
        [FromBody] FeedbackRequest request,
        CancellationToken ct)
    {
        var userId = GetUserId();
        await _chatService.SubmitFeedbackAsync(messageId, userId, request.Score, request.Comment, ct);
        return NoContent();
    }
}

