import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Zone } from '../models/models';

@Injectable({ providedIn: 'root' })
export class ZonesApi {
  private http = inject(HttpClient);
  private base = environment.apiUrl;

  list(top = 50): Observable<Zone[]> {
    return this.http.get<Zone[]>(`${this.base}/zones`, { params: { top } });
  }
}
