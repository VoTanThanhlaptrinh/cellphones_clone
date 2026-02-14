import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, signal, computed } from '@angular/core';
import { map, Observable, tap, catchError, of } from 'rxjs';
import { LoginRequest } from '../core/models/login-request.model';
import { environment } from '../../environments/environment';
import { Router } from '@angular/router';
import { BehaviorSubject } from 'rxjs';
import { jwtDecode } from 'jwt-decode';
import { User } from '../core/models/user.model';

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

  // User Signal to hold the current user state
  currentUser = signal<User | null>(null);

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
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/login`, data, {
      withCredentials: true
    }).pipe(
      map(response => response.data),
      tap(token => {
        this._accessToken = token;
        this.isLoggedInSubject.next(true);
        this.decodeAndSetUser(token);
      })
    );
  }

  refreshToken(skipRedirect: boolean = false): Observable<string | null> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/refresh`, {}, {
      withCredentials: true
    }).pipe(
      map(response => response.data),
      tap(token => {
        this._accessToken = token;
        this.decodeAndSetUser(token);
      }),
      catchError(() => {
        this.clearLocalState();
        return of(null);
      })
    );
  }

  logout() {
    this.http.get(`${this.apiUrl}/logout`, { withCredentials: true }).subscribe({
      next: () => this.handleLogout(),
      error: () => this.handleLogout()
    });

  }

  handleLogout() {
    this.clearLocalState();
  }

  private clearLocalState() {
    this._accessToken = null;
    this.isLoggedInSubject.next(false);
    this.currentUser.set(null);
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

  initializeAuth(): Observable<boolean> {
    return this.http.post<ApiResponse<string>>(`${this.apiUrl}/isLoggedIn`, {}, { withCredentials: true }).pipe(
      tap(response => {
        const token = response.data;
        if (token) {
          this._accessToken = token;
          this.isLoggedInSubject.next(true);
          this.decodeAndSetUser(token);
        }
      }),
      map(() => true),
      catchError((error) => {
        this.isLoggedInSubject.next(false);
        return of(false);
      })
    );
  }

  private decodeAndSetUser(token: string): void {
    try {
      const decoded: any = jwtDecode(token);
      const user: User = {
        id: decoded.nameid,
        fullName: decoded.unique_name,
        roles: decoded.role ? (Array.isArray(decoded.role) ? decoded.role : [decoded.role]) : []
      };
      this.currentUser.set(user);
    } catch (error) {
      this.currentUser.set(null);
    }
  }
}
