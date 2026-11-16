import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { SummaryResult } from '../models';

@Injectable({
  providedIn: 'root'
})
export class SummaryService {
  private readonly baseUrl = `${environment.apiUrl}/api/v1/documents`;

  constructor(private http: HttpClient) {}

  summarizeDocument(documentId: string, summaryType: string): Observable<SummaryResult> {
    return this.http.post<SummaryResult>(`${this.baseUrl}/${documentId}/summarize`, {
      summaryType
    });
  }
}

