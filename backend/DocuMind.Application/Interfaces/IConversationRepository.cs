using DocuMind.Domain.Entities;

namespace DocuMind.Application.Interfaces;

public interface IConversationRepository
{
    Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default);
    Task<Conversation?> GetWithMessagesAsync(Guid conversationId, CancellationToken ct = default);
    Task<Conversation?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Conversation> AddAsync(Conversation conversation, CancellationToken ct = default);
    Task AddMessageAsync(Message message, CancellationToken ct = default);
    Task AddMessageSourceAsync(MessageSource source, CancellationToken ct = default);
    Task AddFeedbackAsync(UserFeedback feedback, CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
