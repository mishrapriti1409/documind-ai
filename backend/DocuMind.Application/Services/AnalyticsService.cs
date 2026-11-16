using DocuMind.Application.DTOs.Analytics;
using DocuMind.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace DocuMind.Application.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _analyticsRepo;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(IAnalyticsRepository analyticsRepo, ILogger<AnalyticsService> logger)
    {
        _analyticsRepo = analyticsRepo;
        _logger = logger;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid organizationId, CancellationToken ct = default)
    {
        _logger.LogInformation("Retrieving dashboard statistics for organization {OrgId}", organizationId);
        return await _analyticsRepo.GetDashboardStatsAsync(organizationId, ct);
    }

    public async Task<IEnumerable<DocumentUploadTrendDto>> GetUploadTrendAsync(Guid organizationId, int days, CancellationToken ct = default)
    {
        days = Math.Clamp(days, 7, 90);
        return await _analyticsRepo.GetUploadTrendAsync(organizationId, days, ct);
    }

    public async Task<IEnumerable<UserActivityDto>> GetUserActivityAsync(Guid organizationId, CancellationToken ct = default)
    {
        return await _analyticsRepo.GetUserActivityAsync(organizationId, ct);
    }
}

