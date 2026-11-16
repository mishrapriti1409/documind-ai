import { Component, EventEmitter, Output } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DocumentService } from '../../../core/services/document.service';

@Component({
  selector: 'app-document-upload',
  templateUrl: './document-upload.component.html',
  styleUrls: ['./document-upload.component.scss']
})
export class DocumentUploadComponent {
  @Output() uploaded = new EventEmitter<void>();
  @Output() cancelled = new EventEmitter<void>();

  uploadForm: FormGroup;
  selectedFile: File | null = null;
  isDragging = false;
  isUploading = false;
  errorMessage: string | null = null;

  categories = [
    'General',
    'HR Policies',
    'Legal & Contracts',
    'Security & Compliance',
    'Technical Specs',
    'Financial Reports'
  ];

  constructor(
    private fb: FormBuilder,
    private docService: DocumentService
  ) {
    this.uploadForm = this.fb.group({
      category: ['General', Validators.required],
      tags: ['']
    });
  }

  onFileSelected(event: any): void {
    const file = event.target.files?.[0];
    if (file) {
      this.validateAndSetFile(file);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging = false;

    const file = event.dataTransfer?.files?.[0];
    if (file) {
      this.validateAndSetFile(file);
    }
  }

  validateAndSetFile(file: File): void {
    this.errorMessage = null;

    if (file.size > 52428800) {
      this.errorMessage = 'File exceeds maximum limit of 50 MB.';
      return;
    }

    const validExts = ['.pdf', '.docx', '.doc', '.xlsx', '.xls', '.txt', '.csv'];
    const fileName = file.name.toLowerCase();
    const isValid = validExts.some(ext => fileName.endsWith(ext));

    if (!isValid) {
      this.errorMessage = 'Unsupported format. Allowed: PDF, DOCX, XLSX, TXT, CSV.';
      return;
    }

    this.selectedFile = file;
  }

  removeFile(): void {
    this.selectedFile = null;
    this.errorMessage = null;
  }

  submitUpload(): void {
    if (!this.selectedFile) {
      this.errorMessage = 'Please select a document to upload.';
      return;
    }

    this.isUploading = true;
    this.errorMessage = null;

    const { category, tags } = this.uploadForm.value;

    this.docService.upload(this.selectedFile, category, tags).subscribe({
      next: () => {
        this.isUploading = false;
        this.uploaded.emit();
      },
      error: err => {
        this.isUploading = false;
        this.errorMessage = err.error?.detail || err.error?.message || 'Failed to upload document.';
      }
    });
  }

  formatBytes(bytes: number): string {
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
  }
}

