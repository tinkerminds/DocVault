namespace DocVault.Api.Models;

/// <summary>
/// DTO returned by GET /api/analytics/summary.
/// Populated by querying the Application Insights REST API.
/// </summary>
public class AnalyticsSummary
{
    /// <summary>Total number of documents uploaded in the last 30 days.</summary>
    public int TotalUploads { get; set; }

    /// <summary>Average time (in milliseconds) taken to process a document upload request, over the last 7 days.</summary>
    public double AvgProcessingTimeMs { get; set; }

    /// <summary>Percentage of API requests that returned HTTP 4xx or 5xx in the last 7 days.</summary>
    public double ErrorRatePercent { get; set; }

    /// <summary>Number of unique authenticated users who have made at least one request in the last 7 days.</summary>
    public int ActiveUsers { get; set; }

    /// <summary>ISO 8601 timestamp of when this summary was generated.</summary>
    public string GeneratedAt { get; set; } = DateTime.UtcNow.ToString("o");
}
