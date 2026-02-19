using DocVault.Api.Models;
using DocVault.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocVault.Api.Controllers;

/// <summary>
/// Provides platform-wide analytics pulled from Application Insights.
/// Requires an authenticated user — open to all logged-in employees.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _analyticsService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IAnalyticsService analyticsService, ILogger<AnalyticsController> logger)
    {
        _analyticsService = analyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Returns a summary of platform metrics from Application Insights.
    /// GET /api/analytics/summary
    /// </summary>
    [HttpGet("summary")]
    public async Task<ActionResult<AnalyticsSummary>> GetSummary()
    {
        _logger.LogInformation("Analytics summary requested by user {UserId}",
            User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value ?? "unknown");

        var summary = await _analyticsService.GetSummaryAsync();
        return Ok(summary);
    }
}
