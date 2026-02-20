using DocVault.Api.Models;

namespace DocVault.Api.Services;

/// <summary>
/// Queries the Application Insights REST API to retrieve platform-wide metrics.
/// </summary>
public interface IAnalyticsService
{
    /// <summary>
    /// Returns a summary of key platform metrics:
    /// total uploads, avg processing time, error rate, and active users.
    /// </summary>
    Task<AnalyticsSummary> GetSummaryAsync();
}
