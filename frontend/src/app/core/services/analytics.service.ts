import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DashboardStats, DocumentUploadTrend, UserActivity } from '../models';

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private readonly apiUrl = `${environment.apiUrl}/api/v1/analytics`;

  constructor(private http: HttpClient) {}

  getDashboardStats(): Observable<DashboardStats> {
    return this.http.get<DashboardStats>(`${this.apiUrl}/dashboard`);
  }

  getUploadTrend(days: number = 30): Observable<DocumentUploadTrend[]> {
    const params = new HttpParams().set('days', days.toString());
    return this.http.get<DocumentUploadTrend[]>(`${this.apiUrl}/uploads-trend`, { params });
  }

  getUserActivity(): Observable<UserActivity[]> {
    return this.http.get<UserActivity[]>(`${this.apiUrl}/user-activity`);
  }
}

