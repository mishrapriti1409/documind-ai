import { Component, OnDestroy, OnInit } from '@angular/core';
import { DocumentService } from '../../../core/services/document.service';
import { AuthService } from '../../../core/services/auth.service';
import { Document } from '../../../core/models';
import { Subscription, interval } from 'rxjs';

@Component({
  selector: 'app-document-list',
  templateUrl: './document-list.component.html',
  styleUrls: ['./document-list.component.scss']
})
export class DocumentListComponent implements OnInit, OnDestroy {
  documents: Document[] = [];
  total = 0;
  page = 1;
  pageSize = 15;

  categoryFilter = '';
  statusFilter = '';

  isUploadVisible = false;
  isLoading = true;
  actionMessage: string | null = null;

  private pollSubscription?: Subscription;

  categories = [
    'General',
    'HR Policies',
    'Legal & Contracts',
    'Security & Compliance',
    'Technical Specs',
    'Financial Reports'
  ];

  statuses = ['Uploaded', 'Pending', 'Processing', 'Completed', 'Failed'];

  constructor(
    private docService: DocumentService,
    public auth: AuthService
  ) {}

  ngOnInit(): void {
    this.loadDocuments();
    // Poll every 8s to update processing statuses
    this.pollSubscription = interval(8000).subscribe(() => {
      const hasActive = this.documents.some(d => d.processingStatus === 'Pending' || d.processingStatus === 'Processing');
      if (hasActive) {
        this.loadDocuments(false);
      }
    });
  }

  ngOnDestroy(): void {
    this.pollSubscription?.unsubscribe();
  }

  loadDocuments(showLoading: boolean = true): void {
    if (showLoading) this.isLoading = true;

    this.docService.getList(this.page, this.pageSize, this.categoryFilter, this.statusFilter).subscribe({
      next: res => {
        this.documents = res.items;
        this.total = res.total;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  onFilterChange(): void {
    this.page = 1;
    this.loadDocuments();
  }

  onPageChange(newPage: number): void {
    this.page = newPage;
    this.loadDocuments();
  }

  deleteDocument(doc: Document): void {
    if (!confirm(`Are you sure you want to delete '${doc.originalFileName}'? Associated vector chunks will also be removed.`)) {
      return;
    }

    this.docService.delete(doc.id).subscribe({
      next: () => {
        this.actionMessage = `Document '${doc.originalFileName}' deleted.`;
        this.loadDocuments();
        setTimeout(() => this.actionMessage = null, 3500);
      }
    });
  }

  retryProcessing(doc: Document): void {
    this.docService.retryProcessing(doc.id).subscribe({
      next: () => {
        this.actionMessage = `Ingestion retry queued for '${doc.originalFileName}'.`;
        this.loadDocuments(false);
        setTimeout(() => this.actionMessage = null, 3500);
      }
    });
  }

  onUploaded(): void {
    this.isUploadVisible = false;
    this.actionMessage = 'Document uploaded successfully! Background parsing and embedding started.';
    this.loadDocuments();
    setTimeout(() => this.actionMessage = null, 5000);
  }

  formatBytes(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
  }
}

