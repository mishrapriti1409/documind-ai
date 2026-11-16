import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DocumentService } from '../../core/services/document.service';
import { SummaryService } from '../../core/services/summary.service';
import { Document, SummaryResult } from '../../core/models';

interface SummaryTypeOption {
  id: string;
  label: string;
  description: string;
  icon: string;
}

@Component({
  selector: 'app-summary',
  templateUrl: './summary.component.html',
  styleUrls: ['./summary.component.scss']
})
export class SummaryComponent implements OnInit {
  documents: Document[] = [];
  selectedDocId: string = '';
  selectedType: string = 'executive';
  isLoadingDocs = false;
  isGenerating = false;
  errorMessage = '';
  summaryResult: SummaryResult | null = null;
  copied = false;

  readonly summaryTypes: SummaryTypeOption[] = [
    { id: 'executive', label: 'Executive Summary', description: 'Strategic high-level overview for leadership', icon: 'briefcase' },
    { id: 'short', label: 'Short Overview', description: '3-5 sentence concise core summary', icon: 'align-left' },
    { id: 'detailed', label: 'Detailed Breakdown', description: 'Comprehensive section-by-section breakdown', icon: 'file-text' },
    { id: 'bullets', label: 'Key Points', description: 'Concise bulleted takeaways & facts', icon: 'list' },
    { id: 'faqs', label: 'Key FAQs', description: 'Common questions & factual answers extracted', icon: 'help-circle' },
    { id: 'dates', label: 'Dates & Deadlines', description: 'Milestones, expiration, and key schedule dates', icon: 'calendar' },
    { id: 'monetary', label: 'Financial & Monetary', description: 'Pricing, penalties, fee caps, payment terms', icon: 'dollar-sign' },
    { id: 'obligations', label: 'Contractual Obligations', description: 'Deliverables, commitments, and liabilities', icon: 'shield' },
    { id: 'entities', label: 'Entities & Parties', description: 'Companies, individuals, and jurisdictions', icon: 'users' }
  ];

  constructor(
    private documentService: DocumentService,
    private summaryService: SummaryService,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.loadDocuments();
  }

  loadDocuments(): void {
    this.isLoadingDocs = true;
    this.documentService.getDocuments(1, 100, undefined, 'Completed').subscribe({
      next: (res: any) => {
        this.documents = res.items;
        this.isLoadingDocs = false;

        const routeDocId = this.route.snapshot.queryParamMap.get('documentId');
        if (routeDocId && this.documents.some(d => d.id === routeDocId)) {
          this.selectedDocId = routeDocId;
        } else if (this.documents.length > 0) {
          this.selectedDocId = this.documents[0].id;
        }
      },
      error: () => {
        this.errorMessage = 'Failed to load completed documents.';
        this.isLoadingDocs = false;
      }
    });
  }

  generateSummary(): void {
    if (!this.selectedDocId) {
      this.errorMessage = 'Please select a document to summarize.';
      return;
    }

    this.isGenerating = true;
    this.errorMessage = '';
    this.summaryResult = null;
    this.copied = false;

    this.summaryService.summarizeDocument(this.selectedDocId, this.selectedType).subscribe({
      next: (res: any) => {
        this.summaryResult = res;
        this.isGenerating = false;
      },
      error: (err: any) => {
        this.errorMessage = err.error?.message || 'Failed to generate summary. Please try again.';
        this.isGenerating = false;
      }
    });
  }

  copyContent(): void {
    if (!this.summaryResult) return;
    const text = typeof this.summaryResult.content === 'string'
      ? this.summaryResult.content
      : JSON.stringify(this.summaryResult.content, null, 2);

    navigator.clipboard.writeText(text).then(() => {
      this.copied = true;
      setTimeout(() => (this.copied = false), 2500);
    });
  }

  isObjectContent(): boolean {
    return !!this.summaryResult && typeof this.summaryResult.content === 'object' && this.summaryResult.content !== null;
  }

  isArrayContent(): boolean {
    return !!this.summaryResult && Array.isArray(this.summaryResult.content);
  }

  getSelectedDocName(): string {
    const doc = this.documents.find(d => d.id === this.selectedDocId);
    return doc ? doc.originalFileName : 'Document';
  }

  getTypeLabel(typeId: string): string {
    const t = this.summaryTypes.find(x => x.id === typeId);
    return t ? t.label : typeId;
  }
}
