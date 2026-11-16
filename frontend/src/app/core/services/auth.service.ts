import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { LoginResponse, RegisterResponse, User } from '../models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly apiUrl = `${environment.apiUrl}/api/v1/auth`;
  private currentUserSubject = new BehaviorSubject<User | null>(this.getStoredUser());
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  public get currentUserValue(): User | null {
    return this.currentUserSubject.value;
  }

  public get isAuthenticated(): boolean {
    return !!this.currentUserValue && !!this.getToken();
  }

  public getToken(): string | null {
    return localStorage.getItem('documind_token');
  }

  login(credentials: { email: string; password: string }): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.apiUrl}/login`, credentials).pipe(
      tap(res => {
        localStorage.setItem('documind_token', res.token);
        localStorage.setItem('documind_user', JSON.stringify(res.user));
        this.currentUserSubject.next(res.user);
      })
    );
  }

  register(payload: {
    email: string;
    password: string;
    firstName: string;
    lastName: string;
    organizationName: string;
  }): Observable<RegisterResponse> {
    return this.http.post<RegisterResponse>(`${this.apiUrl}/register`, payload).pipe(
      tap(res => {
        localStorage.setItem('documind_token', res.token);
        const user: User = {
          id: res.userId,
          email: res.email,
          firstName: payload.firstName,
          lastName: payload.lastName,
          role: res.role as any,
          organizationId: res.organizationId,
          isActive: true
        };
        localStorage.setItem('documind_user', JSON.stringify(user));
        this.currentUserSubject.next(user);
      })
    );
  }

  logout(): void {
    localStorage.removeItem('documind_token');
    localStorage.removeItem('documind_user');
    this.currentUserSubject.next(null);
    this.router.navigate(['/auth/login']);
  }

  changePassword(passwords: { currentPassword: string; newPassword: string }): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/change-password`, passwords);
  }

  hasRole(...roles: string[]): boolean {
    const user = this.currentUserValue;
    return !!user && roles.includes(user.role);
  }

  private getStoredUser(): User | null {
    try {
      const data = localStorage.getItem('documind_user');
      return data ? JSON.parse(data) : null;
    } catch {
      return null;
    }
  }
}

