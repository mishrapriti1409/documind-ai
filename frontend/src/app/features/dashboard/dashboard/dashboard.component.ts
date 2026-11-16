import { Component, OnInit } from '@angular/core';
import { AnalyticsService } from '../../../core/services/analytics.service';
import { DocumentService } from '../../../core/services/document.service';
import { DashboardStats, Document } from '../../../core/models';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  stats: DashboardStats | null = null;
  recentDocs: Document[] = [];
  isLoading = true;

  constructor(
    private analytics: AnalyticsService,
    private documentService: DocumentService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;

    this.analytics.getDashboardStats().subscribe({
      next: data => {
        this.stats = data;
      },
      error: () => {
        // Fallback placeholder stats if API is just initialised
        this.stats = {
          totalDocuments: 0,
          processedDocuments: 0,
          failedDocuments: 0,
          pendingDocuments: 0,
          totalConversations: 0,
          totalMessages: 0,
          activeUsersLast30Days: 1,
          averageProcessingTimeSeconds: 0,
          totalAICostEstimate: 0
        };
      }
    });

    this.documentService.getList(1, 5).subscribe({
      next: res => {
        this.recentDocs = res.items;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  formatBytes(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
  }
}

