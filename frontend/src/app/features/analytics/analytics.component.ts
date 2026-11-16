import { Component, OnInit, ElementRef, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { AnalyticsService } from '../../core/services/analytics.service';
import { DashboardStats, DocumentUploadTrend, UserActivity } from '../../core/models';
import { Chart, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-analytics',
  templateUrl: './analytics.component.html',
  styleUrls: ['./analytics.component.scss']
})
export class AnalyticsComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('trendChart') trendChartCanvas!: ElementRef<HTMLCanvasElement>;

  stats: DashboardStats | null = null;
  trendData: DocumentUploadTrend[] = [];
  userActivity: UserActivity[] = [];
  isLoadingStats = true;
  isLoadingTrend = true;
  isLoadingUsers = true;
  errorMessage = '';
  selectedDays = 30;

  private chart: Chart | null = null;

  constructor(private analyticsService: AnalyticsService) {}

  ngOnInit(): void {
    this.loadDashboardStats();
    this.loadUserActivity();
  }

  ngAfterViewInit(): void {
    this.loadTrendData();
  }

  ngOnDestroy(): void {
    if (this.chart) {
      this.chart.destroy();
    }
  }

  loadDashboardStats(): void {
    this.isLoadingStats = true;
    this.analyticsService.getDashboardStats().subscribe({
      next: (data: DashboardStats) => {
        this.stats = data;
        this.isLoadingStats = false;
      },
      error: () => {
        this.errorMessage = 'Failed to load platform KPI statistics.';
        this.isLoadingStats = false;
      }
    });
  }

  loadTrendData(): void {
    this.isLoadingTrend = true;
    this.analyticsService.getUploadTrend(this.selectedDays).subscribe({
      next: (data: DocumentUploadTrend[]) => {
        this.trendData = data;
        this.isLoadingTrend = false;
        this.renderChart();
      },
      error: () => {
        this.isLoadingTrend = false;
      }
    });
  }

  loadUserActivity(): void {
    this.isLoadingUsers = true;
    this.analyticsService.getUserActivity().subscribe({
      next: (data: UserActivity[]) => {
        this.userActivity = data;
        this.isLoadingUsers = false;
      },
      error: () => {
        this.isLoadingUsers = false;
      }
    });
  }

  onDaysChange(days: number): void {
    this.selectedDays = days;
    this.loadTrendData();
  }

  private renderChart(): void {
    if (!this.trendChartCanvas) return;

    if (this.chart) {
      this.chart.destroy();
    }

    const labels = this.trendData.map(d => {
      const dt = new Date(d.date);
      return dt.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
    });
    const counts = this.trendData.map(d => d.count);

    const ctx = this.trendChartCanvas.nativeElement.getContext('2d');
    if (!ctx) return;

    this.chart = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [
          {
            label: 'Documents Uploaded',
            data: counts,
            backgroundColor: 'rgba(59, 130, 246, 0.75)',
            borderColor: 'rgb(37, 99, 235)',
            borderWidth: 1,
            borderRadius: 4
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          },
          tooltip: {
            backgroundColor: '#1e293b',
            titleFont: { size: 13, weight: 'bold' },
            bodyFont: { size: 12 },
            padding: 10,
            cornerRadius: 6
          }
        },
        scales: {
          x: {
            grid: {
              display: false
            },
            ticks: {
              color: '#64748b',
              font: { size: 11 }
            }
          },
          y: {
            beginAtZero: true,
            ticks: {
              precision: 0,
              color: '#64748b',
              font: { size: 11 }
            },
            grid: {
              color: '#e2e8f0'
            }
          }
        }
      }
    });
  }
}
