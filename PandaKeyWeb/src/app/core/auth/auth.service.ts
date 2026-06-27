import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, Role, User } from '../models/models';
import { TokenStorage } from './token-storage';

/**
 * Reactive authentication state built on Angular signals. The current user and
 * role are exposed as signals so guards, layouts and components react to
 * login/logout without manual subscriptions.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private http = inject(HttpClient);
  private tokens = inject(TokenStorage);
  private base = environment.apiUrl;

  private _user = signal<User | null>(this.decodeFromStorage());
  readonly user = this._user.asReadonly();
  readonly role = computed<Role | null>(() => this._user()?.role ?? null);
  readonly isAuthenticated = computed(() => this._user() !== null);
  readonly isAdmin = computed(() => this._user()?.role === 'admin');

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.base}/auth/login`, { email, password })
      .pipe(tap((res) => this.apply(res)));
  }

  register(fullName: string, email: string, phone: string, password: string): Observable<AuthResponse> {
    return this.http.post<AuthResponse>(`${this.base}/auth/register`, { fullName, email, phone, password })
      .pipe(tap((res) => this.apply(res)));
  }

  logout(): void {
    this.tokens.clear();
    localStorage.removeItem('pk_user');
    this._user.set(null);
  }

  /** Called by the interceptor on a 401 to drop the stale session. */
  forceLogout(): void {
    this.logout();
  }

  private apply(res: AuthResponse): void {
    this.tokens.set(res.token);
    localStorage.setItem('pk_user', JSON.stringify(res.user));
    this._user.set(res.user);
  }

  private decodeFromStorage(): User | null {
    const raw = localStorage.getItem('pk_user');
    if (!raw || !this.tokens.get()) return null;
    try { return JSON.parse(raw) as User; } catch { return null; }
  }
}
