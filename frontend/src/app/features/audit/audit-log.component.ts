import { Component, OnInit } from '@angular/core';
import { AuditService } from '../../core/services/audit.service';
import { AuditLog } from '../../core/models';

@Component({
  selector: 'app-audit-log',
  templateUrl: './audit-log.component.html',
  styleUrls: ['./audit-log.component.scss']
})
export class AuditLogComponent implements OnInit {
  logs: AuditLog[] = [];
  total = 0;
  page = 1;
  pageSize = 25;
  totalPages = 1;
  isLoading = true;
  errorMessage = '';

  filterAction = '';
  selectedLogDetails: AuditLog | null = null;

  readonly actions = [
    'DocumentUploaded',
    'DocumentDeleted',
    'DocumentShared',
    'DocumentReprocessed',
    'QueryExecuted',
    'SummaryGenerated',
    'ComparisonExecuted',
    'UserCreated',
    'RoleUpdated',
    'UserDeactivated'
  ];

  constructor(private auditService: AuditService) {}

  ngOnInit(): void {
    this.loadLogs();
  }

  loadLogs(): void {
    this.isLoading = true;
    this.auditService.getAuditLogs({
      page: this.page,
      pageSize: this.pageSize,
      action: this.filterAction || undefined
    }).subscribe({
      next: (res: any) => {
        this.logs = res.items;
        this.total = res.total;
        this.totalPages = Math.max(1, Math.ceil(this.total / this.pageSize));
        this.isLoading = false;
      },
      error: (err: any) => {
        this.errorMessage = err.error?.message || 'Failed to load tenant audit trail.';
        this.isLoading = false;
      }
    });
  }

  onActionFilterChange(): void {
    this.page = 1;
    this.loadLogs();
  }

  goToPage(p: number): void {
    if (p < 1 || p > this.totalPages || p === this.page) return;
    this.page = p;
    this.loadLogs();
  }

  viewDetails(log: AuditLog): void {
    this.selectedLogDetails = log;
  }

  closeDetails(): void {
    this.selectedLogDetails = null;
  }

  formatJson(val?: string): string {
    if (!val) return 'None';
    try {
      const parsed = JSON.parse(val);
      return JSON.stringify(parsed, null, 2);
    } catch {
      return val;
    }
  }
}
