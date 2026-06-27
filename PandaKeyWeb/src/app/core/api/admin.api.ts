import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Role, User, Zone } from '../models/models';

/** Admin-only endpoints (require a JWT with role=admin). */
@Injectable({ providedIn: 'root' })
export class AdminApi {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/admin`;

  users(): Observable<User[]> {
    return this.http.get<User[]>(`${this.base}/users`);
  }
  setRole(userId: number, role: Role): Observable<void> {
    return this.http.post<void>(`${this.base}/users/${userId}/role`, { role });
  }
  deleteUser(userId: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/users/${userId}`);
  }
  zones(): Observable<Zone[]> {
    return this.http.get<Zone[]>(`${this.base}/zones`);
  }
  createZone(name: string, description: string): Observable<{ zoneId: number }> {
    return this.http.post<{ zoneId: number }>(`${this.base}/zones`, { name, description });
  }
  deleteZone(zoneId: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/zones/${zoneId}`);
  }
}
