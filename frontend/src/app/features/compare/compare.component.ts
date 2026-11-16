import { Component, OnInit } from '@angular/core';
import { DocumentService } from '../../core/services/document.service';
import { CompareService } from '../../core/services/compare.service';
import { Document, ComparisonResult } from '../../core/models';

@Component({
  selector: 'app-compare',
  templateUrl: './compare.component.html',
  styleUrls: ['./compare.component.scss']
})
export class CompareComponent implements OnInit {
  documents: Document[] = [];
  selectedDocA: string = '';
  selectedDocB: string = '';
  isLoadingDocs = false;
  isComparing = false;
  errorMessage = '';
  comparisonResult: ComparisonResult | null = null;
  activeTab: 'all' | 'added' | 'removed' | 'modified' = 'all';

  constructor(
    private documentService: DocumentService,
    private compareService: CompareService
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
        if (this.documents.length >= 2) {
          this.selectedDocA = this.documents[0].id;
          this.selectedDocB = this.documents[1].id;
        }
      },
      error: (err: any) => {
        this.errorMessage = 'Failed to load completed documents for comparison.';
        this.isLoadingDocs = false;
      }
    });
  }

  runComparison(): void {
    if (!this.selectedDocA || !this.selectedDocB) {
      this.errorMessage = 'Please select both Document A and Document B to compare.';
      return;
    }

    if (this.selectedDocA === this.selectedDocB) {
      this.errorMessage = 'Please select two different documents to perform comparison.';
      return;
    }

    this.isComparing = true;
    this.errorMessage = '';
    this.comparisonResult = null;

    this.compareService.compareDocuments(this.selectedDocA, this.selectedDocB).subscribe({
      next: (res: any) => {
        this.comparisonResult = res;
        this.isComparing = false;
      },
      error: (err: any) => {
        this.errorMessage = err.error?.message || 'Comparison failed. Please verify both documents are fully processed.';
        this.isComparing = false;
      }
    });
  }

  getDocName(id: string): string {
    const doc = this.documents.find(d => d.id === id);
    return doc ? doc.originalFileName : 'Document';
  }
}
