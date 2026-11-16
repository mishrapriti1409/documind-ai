namespace DocuMind.Domain.Entities;

/// <summary>
/// User rating and qualitative feedback for AI answers.
/// </summary>
public class UserFeedback
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid MessageId { get; set; }
    public Guid UserId { get; set; }
    public int Score { get; set; } // 1 (thumbs down) to 5 (excellent)
    public string? Comment { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navigation
    public Message? Message { get; set; }
    public ApplicationUser? User { get; set; }
}

