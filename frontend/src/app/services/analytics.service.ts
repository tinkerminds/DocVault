import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, catchError, of } from 'rxjs';
import { environment } from '../../environments/environment';
import { AnalyticsSummary } from '../models/analytics-summary.model';

@Injectable({
    providedIn: 'root',
})
export class AnalyticsService {
    private readonly apiUrl = `${environment.apiBaseUrl}/analytics/summary`;

    constructor(private http: HttpClient) { }

    /**
     * Fetches platform analytics from GET /api/analytics/summary.
     * The backend queries Application Insights and returns the result.
     */
    getAnalyticsSummary(): Observable<AnalyticsSummary> {
        return this.http.get<AnalyticsSummary>(this.apiUrl).pipe(
            catchError((err) => {
                console.error('Failed to load analytics summary:', err);
                // Return empty summary so the dashboard renders with zeros
                return of({
                    totalUploads: 0,
                    avgProcessingTimeMs: 0,
                    errorRatePercent: 0,
                    activeUsers: 0,
                    generatedAt: new Date().toISOString(),
                } as AnalyticsSummary);
            })
        );
    }
}
