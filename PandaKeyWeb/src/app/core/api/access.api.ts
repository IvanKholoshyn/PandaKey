import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AccessDecision } from '../models/models';

@Injectable({ providedIn: 'root' })
export class AccessApi {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  decide(userId: number, accessPointId: number): Observable<AccessDecision> {
    return this.http.post<AccessDecision>(`${this.base}/access/decide`, { userId, accessPointId });
  }
}
