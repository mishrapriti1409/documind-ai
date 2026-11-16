using DocuMind.Domain.Entities;
using DocuMind.Infrastructure.Data;
using Microsoft.Extensions.Logging;

namespace DocuMind.Infrastructure.Services;

public interface IAuditService
{
    Task LogAsync(
        Guid? userId,
        Guid? organizationId,
        string action,
        string resourceType,
        Guid? resourceId = null,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        CancellationToken ct = default);
}

public sealed class AuditService : IAuditService
{
    private readonly AppDbContext _context;
    private readonly ILogger<AuditService> _logger;

    public AuditService(AppDbContext context, ILogger<AuditService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task LogAsync(
        Guid? userId,
        Guid? organizationId,
        string action,
        string resourceType,
        Guid? resourceId = null,
        string? oldValues = null,
        string? newValues = null,
        string? ipAddress = null,
        CancellationToken ct = default)
    {
        var log = new AuditLog
        {
            UserId = userId,
            OrganizationId = organizationId,
            Action = action,
            ResourceType = resourceType,
            ResourceId = resourceId,
            OldValues = oldValues,
            NewValues = newValues,
            IpAddress = ipAddress,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _context.AuditLogs.Add(log);
        await _context.SaveChangesAsync(ct);
        _logger.LogInformation("Audit recorded: {Action} on {Resource} by User {UserId}", action, resourceType, userId);
    }
}

