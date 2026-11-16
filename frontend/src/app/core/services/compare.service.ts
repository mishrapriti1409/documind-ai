import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ComparisonResult } from '../models';

@Injectable({
  providedIn: 'root'
})
export class CompareService {
  private readonly apiUrl = `${environment.apiUrl}/api/v1/compare`;

  constructor(private http: HttpClient) {}

  compareDocuments(documentIdA: string, documentIdB: string): Observable<ComparisonResult> {
    return this.http.post<ComparisonResult>(this.apiUrl, {
      documentIdA,
      documentIdB
    });
  }
}

