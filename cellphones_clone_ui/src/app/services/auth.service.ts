import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map, Observable, tap, catchError, of } from 'rxjs';
import { LoginRequest } from '../core/models/login-request.model';
import { environment } from '../../environments/environment';
import { Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';

export interface ApiResponse<T> {
  message: string;
  data: T;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = `${environment.apiUrl}/auth`;

  private _accessToken: string | null = null;
  private isLoggedInSubject = new BehaviorSubject<boolean>(false);

  constructor(
    private http: HttpClient,
    private router: Router
  ) { }

  get accessToken(): string | null {
    return this._accessToken;
  }

  get isLoggedIn$(): Observable<boolean> {
    return this.isLoggedInSubject.asObservable();
  }

  register(data: any): Observable<void> {
    return this.http.post<ApiResponse<void>>(`${this.apiUrl}/register`, data).pipe(
      map(response => response.data)
    );
  }

  login(data: LoginRequest): Observable<string> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/login`, data).pipe(
      map(response => response.data),
      tap(token => {
        this._accessToken = token;
        this.isLoggedInSubject.next(true);
      })
    );
  }

  refreshToken(): Observable<string | null> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/refresh-token`, {}, {
      withCredentials: true
    }).pipe(
      map(response => response.data),
      tap(token => {
        this._accessToken = token;
        this.isLoggedInSubject.next(true);
      }),
      catchError(() => {
        this.logout();
        return of(null);
      })
    );
  }

  logout() {
    this.http.post(`${this.apiUrl}/logout`, {}, { withCredentials: true }).subscribe({
      next: () => this.handleLogout(),
      error: () => this.handleLogout()
    });
  }

  private handleLogout() {
    this._accessToken = null;
    this.isLoggedInSubject.next(false);
    this.router.navigate(['/login']);
  }

  studentRegister(data: any): Observable<string> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/studentRegister`, data).pipe(
      map(response => response.data)
    );
  }

  teacherRegister(data: any): Observable<string> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/teacherRegister`, data).pipe(
      map(response => response.data)
    );
  }
}
