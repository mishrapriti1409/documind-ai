using DocuMind.Application.Interfaces;
using DocuMind.Domain.Entities;
using DocuMind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DocuMind.Infrastructure.Repositories;

public class ConversationRepository : Repository<Conversation>, IConversationRepository
{
    public ConversationRepository(AppDbContext context) : base(context) { }

    public async Task<IEnumerable<Conversation>> GetUserConversationsAsync(Guid userId, int page, int pageSize, CancellationToken ct = default)
    {
        return await _context.Conversations
            .Include(c => c.Messages)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.UpdatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<Conversation?> GetWithMessagesAsync(Guid conversationId, CancellationToken ct = default)
    {
        return await _context.Conversations
            .Include(c => c.Messages.OrderBy(m => m.CreatedAt))
                .ThenInclude(m => m.Sources)
            .Include(c => c.Messages)
                .ThenInclude(m => m.Feedback)
            .FirstOrDefaultAsync(c => c.Id == conversationId, ct);
    }

    public async Task AddMessageAsync(Message message, CancellationToken ct = default)
    {
        await _context.Messages.AddAsync(message, ct);
    }

    public async Task AddMessageSourceAsync(MessageSource source, CancellationToken ct = default)
    {
        await _context.MessageSources.AddAsync(source, ct);
    }

    public async Task AddFeedbackAsync(UserFeedback feedback, CancellationToken ct = default)
    {
        await _context.UserFeedbacks.AddAsync(feedback, ct);
    }
}
