using DocuMind.Application.DTOs.Analytics;
using DocuMind.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocuMind.API.Controllers;

[ApiController]
[Route("api/v1/analytics")]
[Authorize(Roles = "Admin,Manager")]
[Produces("application/json")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    private Guid GetOrganizationId()
    {
        var claim = User.FindFirstValue("OrganizationId");
        return string.IsNullOrEmpty(claim) ? Guid.Empty : Guid.Parse(claim);
    }

    /// <summary>Get high-level organizational analytics, processing throughput, and AI cost metrics.</summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardStatsDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboard(CancellationToken ct)
    {
        var orgId = GetOrganizationId();
        var stats = await _analyticsService.GetDashboardStatsAsync(orgId, ct);
        return Ok(stats);
    }

    /// <summary>Get document upload volume trend over the specified number of days (default 30).</summary>
    [HttpGet("uploads-trend")]
    [ProducesResponseType(typeof(IEnumerable<DocumentUploadTrendDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DocumentUploadTrendDto>>> GetUploadTrend(
        [FromQuery] int days = 30,
        CancellationToken ct = default)
    {
        var orgId = GetOrganizationId();
        var trend = await _analyticsService.GetUploadTrendAsync(orgId, days, ct);
        return Ok(trend);
    }

    /// <summary>Get per-user document activity and engagement metrics.</summary>
    [HttpGet("user-activity")]
    [ProducesResponseType(typeof(IEnumerable<UserActivityDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<UserActivityDto>>> GetUserActivity(CancellationToken ct)
    {
        var orgId = GetOrganizationId();
        var activity = await _analyticsService.GetUserActivityAsync(orgId, ct);
        return Ok(activity);
    }
}

