import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuditLogResponse } from '../models';

@Injectable({
  providedIn: 'root'
})
export class AuditService {
  private readonly apiUrl = `${environment.apiUrl}/api/v1/audit-logs`;

  constructor(private http: HttpClient) {}

  getAuditLogs(params: {
    page?: number;
    pageSize?: number;
    action?: string;
    userId?: string;
    from?: string;
    to?: string;
  } = {}): Observable<AuditLogResponse> {
    let httpParams = new HttpParams()
      .set('page', (params.page || 1).toString())
      .set('pageSize', (params.pageSize || 50).toString());

    if (params.action) {
      httpParams = httpParams.set('action', params.action);
    }
    if (params.userId) {
      httpParams = httpParams.set('userId', params.userId);
    }
    if (params.from) {
      httpParams = httpParams.set('from', params.from);
    }
    if (params.to) {
      httpParams = httpParams.set('to', params.to);
    }

    return this.http.get<AuditLogResponse>(this.apiUrl, { params: httpParams });
  }
}

