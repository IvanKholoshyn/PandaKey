import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ImportResult } from '../models/models';

@Injectable({ providedIn: 'root' })
export class BackupApi {
  private http = inject(HttpClient);
  private base = `${environment.apiUrl}/admin/backup`;

  /** Downloads the full system dump as a Blob. */
  exportBackup(): Observable<Blob> {
    return this.http.get(`${this.base}/export`, { responseType: 'blob' });
  }

  importBackup(file: File): Observable<ImportResult> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<ImportResult>(`${this.base}/import`, form);
  }
}
