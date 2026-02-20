using System.Text;
using System.Text.Json;
using DocVault.Api.Models;

namespace DocVault.Api.Services;

/// <summary>
/// Provides platform analytics by combining two data sources:
///
/// ┌──────────────────────────────────────────────────────────────────┐
/// │  Metric             │  Source     │  Why                         │
/// ├──────────────────────────────────────────────────────────────────┤
/// │  Total Uploads      │  Cosmos DB  │  Real-time, no lag           │
/// │  Active Users       │  Cosmos DB  │  Real-time, no lag           │
/// │  Avg Processing Time│  App Insights│  Telemetry-based, needs key │
/// │  Error Rate         │  App Insights│  Telemetry-based, needs key │
/// └──────────────────────────────────────────────────────────────────┘
///
/// This hybrid approach means the dashboard always shows real document
/// counts from Cosmos DB instantly, while also surfacing App Insights
/// performance metrics when credentials are configured.
/// </summary>
public class AnalyticsService : IAnalyticsService
{
    private readonly ICosmosDbService _cosmosService;
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<AnalyticsService> _logger;

    private const string AppInsightsBaseUrl = "https://api.applicationinsights.io/v1/apps";

    public AnalyticsService(
        ICosmosDbService cosmosService,
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ILogger<AnalyticsService> logger)
    {
        _cosmosService = cosmosService;
        _httpClient = httpClientFactory.CreateClient("AppInsights");
        _config = config;
        _logger = logger;
    }

    public async Task<AnalyticsSummary> GetSummaryAsync()
    {
        // ── 1. Real-time Cosmos DB queries (always work, no credentials needed) ──
        var totalUploadsTask = GetTotalUploadsFromCosmos();
        var activeUsersTask = GetActiveUsersFromCosmos();

        // ── 2. App Insights queries (only if credentials are configured) ──────────
        var appId = _config["AppInsights:AppId"];
        var apiKey = _config["AppInsights:ApiKey"];
        bool appInsightsConfigured = !string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(apiKey);

        Task<double> avgProcessingTask;
        Task<double> errorRateTask;

        if (appInsightsConfigured)
        {
            avgProcessingTask = QueryAppInsightsValueAsync(appId!, apiKey!, AvgProcessingTimeQuery);
            errorRateTask = QueryAppInsightsValueAsync(appId!, apiKey!, ErrorRateQuery);
        }
        else
        {
            _logger.LogInformation(
                "AppInsights credentials not configured — processing time and error rate will show as 0.");
            avgProcessingTask = Task.FromResult(0.0);
            errorRateTask = Task.FromResult(0.0);
        }

        // Run all tasks in parallel
        await Task.WhenAll(totalUploadsTask, activeUsersTask, avgProcessingTask, errorRateTask);

        return new AnalyticsSummary
        {
            TotalUploads = await totalUploadsTask,
            ActiveUsers = await activeUsersTask,
            AvgProcessingTimeMs = Math.Round(await avgProcessingTask, 1),
            ErrorRatePercent = Math.Round(await errorRateTask, 2),
            GeneratedAt = DateTime.UtcNow.ToString("o")
        };
    }

    // ── Cosmos DB helpers ─────────────────────────────────────────────────────

    private async Task<int> GetTotalUploadsFromCosmos()
    {
        try
        {
            return await _cosmosService.GetTotalDocumentCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get total document count from Cosmos DB.");
            return 0;
        }
    }

    private async Task<int> GetActiveUsersFromCosmos()
    {
        try
        {
            return await _cosmosService.GetDistinctUserCountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get distinct user count from Cosmos DB.");
            return 0;
        }
    }

    // ── App Insights Kusto queries ────────────────────────────────────────────

    /// <summary>Average duration in ms of POST /api/documents requests in last 7 days.</summary>
    private const string AvgProcessingTimeQuery = @"
requests
| where timestamp > ago(7d)
| where name contains 'documents' and name contains 'POST'
| summarize avg(duration)
";

    /// <summary>
    /// Error rate = (failed requests / total requests) * 100, in last 7 days.
    /// Returns 0 if no data.
    /// </summary>
    private const string ErrorRateQuery = @"
requests
| where timestamp > ago(7d)
| summarize total = count(), failed = countif(success == false)
| extend errorRate = iif(total == 0, 0.0, (toreal(failed) / toreal(total)) * 100)
| project errorRate
";

    /// <summary>
    /// Calls the App Insights Query REST API and returns the first numeric cell.
    /// POST https://api.applicationinsights.io/v1/apps/{appId}/query
    /// </summary>
    private async Task<double> QueryAppInsightsValueAsync(string appId, string apiKey, string query)
    {
        try
        {
            var url = $"{AppInsightsBaseUrl}/{appId}/query";
            var body = JsonSerializer.Serialize(new { query = query.Trim() });

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-api-key", apiKey);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            // { "tables": [ { "rows": [ [ value ] ] } ] }
            var rows = doc.RootElement
                .GetProperty("tables")[0]
                .GetProperty("rows");

            if (rows.GetArrayLength() == 0) return 0;

            var cell = rows[0][0];
            return cell.ValueKind switch
            {
                JsonValueKind.Number => cell.GetDouble(),
                JsonValueKind.Null => 0,
                _ => 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "App Insights query failed.");
            return 0;
        }
    }
}
