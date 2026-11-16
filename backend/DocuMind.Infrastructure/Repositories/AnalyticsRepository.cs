using DocuMind.Application.DTOs.Analytics;
using DocuMind.Application.Interfaces;
using DocuMind.Domain.Enums;
using DocuMind.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace DocuMind.Infrastructure.Repositories;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly AppDbContext _context;

    public AnalyticsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid organizationId, CancellationToken ct = default)
    {
        var totalDocs = await _context.Documents.CountAsync(d => d.OrganizationId == organizationId, ct);
        var processedDocs = await _context.Documents.CountAsync(d => d.OrganizationId == organizationId && d.ProcessingStatus == ProcessingStatus.Completed, ct);
        var failedDocs = await _context.Documents.CountAsync(d => d.OrganizationId == organizationId && d.ProcessingStatus == ProcessingStatus.Failed, ct);
        var pendingDocs = await _context.Documents.CountAsync(d => d.OrganizationId == organizationId && (d.ProcessingStatus == ProcessingStatus.Pending || d.ProcessingStatus == ProcessingStatus.Processing), ct);

        var totalConversations = await _context.Conversations.CountAsync(c => c.OrganizationId == organizationId, ct);
        var totalMessages = await _context.Messages.CountAsync(m => m.Conversation!.OrganizationId == organizationId, ct);

        var thirtyDaysAgo = DateTimeOffset.UtcNow.AddDays(-30);
        var activeUsers = await _context.Users.CountAsync(u => u.OrganizationId == organizationId && u.LastLoginAt >= thirtyDaysAgo, ct);

        var totalCost = await _context.Messages
            .Where(m => m.Conversation!.OrganizationId == organizationId)
            .SumAsync(m => m.EstimatedCost, ct);

        // Calculate average processing duration for completed jobs
        var completedJobs = await _context.ProcessingJobs
            .Where(j => j.Document!.OrganizationId == organizationId && j.Status == "Completed" && j.StartedAt != null && j.CompletedAt != null)
            .Take(50)
            .Select(j => new { j.StartedAt, j.CompletedAt })
            .ToListAsync(ct);

        double avgProcessingTime = 0;
        if (completedJobs.Any())
        {
            avgProcessingTime = completedJobs
                .Average(j => (j.CompletedAt!.Value - j.StartedAt!.Value).TotalSeconds);
        }

        return new DashboardStatsDto(
            TotalDocuments: totalDocs,
            ProcessedDocuments: processedDocs,
            FailedDocuments: failedDocs,
            PendingDocuments: pendingDocs,
            TotalConversations: totalConversations,
            TotalMessages: totalMessages,
            ActiveUsersLast30Days: activeUsers,
            AverageProcessingTimeSeconds: Math.Round(avgProcessingTime, 1),
            TotalAICostEstimate: Math.Round(totalCost, 4)
        );
    }

    public async Task<IEnumerable<DocumentUploadTrendDto>> GetUploadTrendAsync(Guid organizationId, int days, CancellationToken ct = default)
    {
        var startDate = DateTimeOffset.UtcNow.AddDays(-days);

        var uploads = await _context.Documents
            .Where(d => d.OrganizationId == organizationId && d.CreatedAt >= startDate)
            .Select(d => d.CreatedAt)
            .ToListAsync(ct);

        return uploads
            .GroupBy(d => DateOnly.FromDateTime(d.Date))
            .OrderBy(g => g.Key)
            .Select(g => new DocumentUploadTrendDto(g.Key, g.Count()))
            .ToList();
    }

    public async Task<IEnumerable<UserActivityDto>> GetUserActivityAsync(Guid organizationId, CancellationToken ct = default)
    {
        var users = await _context.Users
            .Where(u => u.OrganizationId == organizationId)
            .Take(20)
            .Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                u.LastLoginAt,
                u.CreatedAt,
                UploadedDocs = _context.Documents.Count(d => d.UploadedByUserId == u.Id),
                ConversationsStarted = _context.Conversations.Count(c => c.UserId == u.Id)
            })
            .ToListAsync(ct);

        return users.Select(u => new UserActivityDto(
            u.Id,
            $"{u.FirstName} {u.LastName}".Trim() is { Length: > 0 } name ? name : u.Email!,
            u.UploadedDocs,
            u.ConversationsStarted,
            u.LastLoginAt ?? u.CreatedAt
        ));
    }
}

