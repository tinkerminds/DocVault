/// Analytics summary data returned by GET /api/analytics/summary
export interface AnalyticsSummary {
    totalUploads: number;
    avgProcessingTimeMs: number;
    errorRatePercent: number;
    activeUsers: number;
    generatedAt: string;
}
