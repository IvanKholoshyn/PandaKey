import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TranslateModule, TranslateService } from '@ngx-translate/core';
import { BackupApi } from '../../core/api/backup.api';
import { ImportResult } from '../../core/models/models';

/**
 * Admin backup screen: exports the whole system as a JSON file and imports a
 * previously exported dump back into the database.
 *
 * Export is a pure client-side download: the API returns a Blob, the component
 * creates an object URL, programmatically clicks a hidden <a download>, then
 * revokes the URL. The file name carries a timestamp so repeated exports do not
 * collide (pandakey-backup-<ts>.json).
 */
@Component({
  selector: 'app-admin-backup',
  standalone: true,
  imports: [CommonModule, TranslateModule],
  template: `
  <div class="row g-4">
    <div class="col-md-6">
      <div class="card h-100">
        <div class="card-body">
          <h5 class="card-title">{{ 'admin.exportTitle' | translate }}</h5>
          <p class="text-muted small">{{ 'admin.exportDesc' | translate }}</p>
          <button class="btn btn-primary" (click)="onExport()" [disabled]="exporting()">
            <span *ngIf="exporting()" class="spinner-border spinner-border-sm me-2"></span>
            {{ 'admin.exportBtn' | translate }}
          </button>
          <div *ngIf="exportError()" class="text-danger small mt-2">{{ exportError() }}</div>
        </div>
      </div>
    </div>

    <div class="col-md-6">
      <div class="card h-100">
        <div class="card-body">
          <h5 class="card-title">{{ 'admin.importTitle' | translate }}</h5>
          <p class="text-muted small">{{ 'admin.importDesc' | translate }}</p>

          <input #fileInput type="file" accept="application/json,.json"
                 class="form-control mb-2" (change)="onFileSelected($event)" />

          <button class="btn btn-outline-primary" (click)="onImport()"
                  [disabled]="!selectedFile() || importing()">
            <span *ngIf="importing()" class="spinner-border spinner-border-sm me-2"></span>
            {{ 'admin.importBtn' | translate }}
          </button>

          <div *ngIf="importDone()" class="alert alert-success mt-3 mb-0 py-2 small">
            {{ importDone() }}
          </div>
          <div *ngIf="importError()" class="text-danger small mt-2">{{ importError() }}</div>
        </div>
      </div>
    </div>
  </div>
  `
})
export class BackupComponent {
  private api = inject(BackupApi);
  private translate = inject(TranslateService);

  exporting = signal(false);
  exportError = signal<string | null>(null);

  selectedFile = signal<File | null>(null);
  importing = signal(false);
  importDone = signal<string | null>(null);
  importError = signal<string | null>(null);

  onExport(): void {
    this.exporting.set(true);
    this.exportError.set(null);
    this.api.exportBackup().subscribe({
      next: (blob) => {
        if (!blob || blob.size === 0) {
          this.exportError.set('Empty backup');
          this.exporting.set(false);
          return;
        }
        const ts = new Date().toISOString().replace(/[:.]/g, '-');
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `pandakey-backup-${ts}.json`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);
        this.exporting.set(false);
      },
      error: () => {
        this.exportError.set('Export failed');
        this.exporting.set(false);
      }
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFile.set(input.files && input.files.length ? input.files[0] : null);
    this.importDone.set(null);
    this.importError.set(null);
  }

  onImport(): void {
    const file = this.selectedFile();
    if (!file) return;
    this.importing.set(true);
    this.importDone.set(null);
    this.importError.set(null);
    this.api.importBackup(file).subscribe({
      next: (res: ImportResult) => {
        const msg = this.translate.instant('admin.importDone', {
          count: res.imported,
          tables: res.tables
        });
        this.importDone.set(msg);
        this.importing.set(false);
      },
      error: () => {
        this.importError.set('Import failed');
        this.importing.set(false);
      }
    });
  }
}
