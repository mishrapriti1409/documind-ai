import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { User } from '../models';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  private readonly apiUrl = `${environment.apiUrl}/api/v1/users`;

  constructor(private http: HttpClient) {}

  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.apiUrl);
  }

  updateRole(userId: string, role: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/${userId}/role`, { role });
  }

  toggleActive(userId: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/${userId}/toggle-active`, {});
  }
}

