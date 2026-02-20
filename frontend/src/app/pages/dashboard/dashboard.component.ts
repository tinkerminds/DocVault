import { Component, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AnalyticsService } from '../../services/analytics.service';
import { AnalyticsSummary } from '../../models/analytics-summary.model';

interface MetricCard {
    label: string;
    value: string;
    icon: string;
    iconColorClass: string;
    description: string;
}

@Component({
    selector: 'app-dashboard',
    standalone: true,
    imports: [CommonModule],
    templateUrl: './dashboard.component.html',
    styleUrl: './dashboard.component.scss',
})
export class DashboardComponent implements OnInit, OnDestroy {
    isLoading = true;
    hasError = false;
    summary: AnalyticsSummary | null = null;
    cards: MetricCard[] = [];
    generatedAt = '';

    private destroy$ = new Subject<void>();

    constructor(
        private analyticsService: AnalyticsService,
        private cdr: ChangeDetectorRef
    ) { }

    ngOnInit(): void {
        this.loadAnalytics();
    }

    ngOnDestroy(): void {
        this.destroy$.next();
        this.destroy$.complete();
    }

    loadAnalytics(): void {
        this.isLoading = true;
        this.hasError = false;
        this.cdr.detectChanges();

        this.analyticsService
            .getAnalyticsSummary()
            .pipe(takeUntil(this.destroy$))
            .subscribe({
                next: (data) => {
                    this.summary = data;
                    this.cards = this.buildCards(data);
                    this.generatedAt = this.formatDate(data.generatedAt);
                    this.isLoading = false;
                    this.hasError = false;
                    this.cdr.detectChanges(); // Force Angular to re-render after MSAL-intercepted response
                },
                error: () => {
                    this.hasError = true;
                    this.isLoading = false;
                    this.cdr.detectChanges();
                },
            });
    }

    private buildCards(data: AnalyticsSummary): MetricCard[] {
        return [
            {
                label: 'Total Uploads',
                value: data.totalUploads.toLocaleString(),
                icon: 'cloud_upload',
                iconColorClass: 'icon-blue',
                description: 'Documents uploaded in the last 30 days',
            },
            {
                label: 'Avg Processing Time',
                value: data.avgProcessingTimeMs === 0
                    ? '—'
                    : `${data.avgProcessingTimeMs.toFixed(0)} ms`,
                icon: 'timer',
                iconColorClass: 'icon-teal',
                description: 'Average upload request duration (7 days)',
            },
            {
                label: 'Error Rate',
                value: `${data.errorRatePercent.toFixed(2)}%`,
                icon: data.errorRatePercent > 5 ? 'warning' : 'check_circle',
                iconColorClass: data.errorRatePercent > 5 ? 'icon-red' : 'icon-green',
                description: 'Requests returning 4xx / 5xx (7 days)',
            },
            {
                label: 'Active Users',
                value: data.activeUsers.toLocaleString(),
                icon: 'group',
                iconColorClass: 'icon-purple',
                description: 'Unique authenticated users (7 days)',
            },
        ];
    }

    private formatDate(iso: string): string {
        try {
            return new Date(iso).toLocaleString('en-IN', {
                day: 'numeric',
                month: 'short',
                year: 'numeric',
                hour: '2-digit',
                minute: '2-digit',
            });
        } catch {
            return '';
        }
    }
}
