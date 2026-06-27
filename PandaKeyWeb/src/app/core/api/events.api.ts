import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AccessEvent } from '../models/models';

@Injectable({ providedIn: 'root' })
export class EventsApi {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  latest(top = 50): Observable<AccessEvent[]> {
    return this.http.get<AccessEvent[]>(`${this.base}/access-events`, { params: { top } });
  }
}
