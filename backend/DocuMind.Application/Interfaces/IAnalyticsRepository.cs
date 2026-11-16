using DocuMind.Application.DTOs.Analytics;

namespace DocuMind.Application.Interfaces;

public interface IAnalyticsRepository
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(Guid organizationId, CancellationToken ct = default);
    Task<IEnumerable<DocumentUploadTrendDto>> GetUploadTrendAsync(Guid organizationId, int days, CancellationToken ct = default);
    Task<IEnumerable<UserActivityDto>> GetUserActivityAsync(Guid organizationId, CancellationToken ct = default);
}

