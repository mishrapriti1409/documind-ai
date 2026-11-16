using DocuMind.Domain.Common;
using DocuMind.Domain.Enums;

namespace DocuMind.Domain.Entities;

/// <summary>
/// Individual message in a conversation thread with token and cost attribution.
/// </summary>
public class Message : BaseEntity
{
    public Guid ConversationId { get; set; }
    public MessageRole Role { get; set; }
    public required string Content { get; set; }
    public int TokensUsed { get; set; }
    public decimal EstimatedCost { get; set; }
    public int? FeedbackScore { get; set; }

    // Navigation
    public Conversation? Conversation { get; set; }
    public ICollection<MessageSource> Sources { get; set; } = new List<MessageSource>();
    public UserFeedback? Feedback { get; set; }
}

