import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Document, DocumentListResponse, ProcessingStatusDto, SummaryResult } from '../models';

@Injectable({
  providedIn: 'root'
})
export class DocumentService {
  private readonly apiUrl = `${environment.apiUrl}/api/v1/documents`;

  constructor(private http: HttpClient) {}

  upload(file: File, category?: string, tags?: string): Observable<Document> {
    const formData = new FormData();
    formData.append('file', file);
    if (category) formData.append('category', category);
    if (tags) formData.append('tags', tags);

    return this.http.post<Document>(`${this.apiUrl}/upload`, formData);
  }

  getList(page: number = 1, pageSize: number = 20, category?: string, status?: string): Observable<DocumentListResponse> {
    let params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    if (category) params = params.set('category', category);
    if (status) params = params.set('status', status);

    return this.http.get<DocumentListResponse>(this.apiUrl, { params });
  }

  getDocuments(page: number = 1, pageSize: number = 20, category?: string, status?: string): Observable<DocumentListResponse> {
    return this.getList(page, pageSize, category, status);
  }

  getById(id: string): Observable<Document> {
    return this.http.get<Document>(`${this.apiUrl}/${id}`);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }

  getStatus(id: string): Observable<ProcessingStatusDto> {
    return this.http.get<ProcessingStatusDto>(`${this.apiUrl}/${id}/status`);
  }

  retryProcessing(id: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/${id}/retry`, {});
  }

  summarize(id: string, summaryType: string = 'short'): Observable<SummaryResult> {
    return this.http.post<SummaryResult>(`${this.apiUrl}/${id}/summarize`, { summaryType });
  }
}
