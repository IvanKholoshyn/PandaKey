import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { AdminApi } from '../../core/api/admin.api';
import { LanguageService } from '../../core/i18n/language.service';
import { Zone } from '../../core/models/models';

@Component({
  selector: 'app-all-zones',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  template: `
  <div class="d-flex justify-content-between align-items-center mb-3">
    <h3 class="m-0">{{ 'admin.allZones' | translate }}</h3>
    <button class="btn btn-sm btn-pk" (click)="creating.set(!creating())">{{ 'admin.addZone' | translate }}</button>
  </div>

  <div class="card mb-3" *ngIf="creating()">
    <div class="card-body row g-2 align-items-end">
      <div class="col-md-4">
        <label class="form-label">{{ 'admin.zone' | translate }}</label>
        <input class="form-control" [(ngModel)]="newName" />
      </div>
      <div class="col-md-6">
        <label class="form-label">{{ 'admin.description' | translate }}</label>
        <input class="form-control" [(ngModel)]="newDesc" />
      </div>
      <div class="col-md-2">
        <button class="btn btn-pk w-100" (click)="create()">{{ 'admin.addZone' | translate }}</button>
      </div>
    </div>
  </div>

  <table class="table align-middle">
    <thead>
      <tr>
        <th>#</th>
        <th>{{ 'admin.zone' | translate }}</th>
        <th>{{ 'admin.description' | translate }}</th>
        <th>{{ 'admin.points' | translate }}</th>
        <th>{{ 'admin.rules' | translate }}</th>
        <th class="text-end">{{ 'admin.actions' | translate }}</th>
      </tr>
    </thead>
    <tbody>
      <tr *ngFor="let z of sorted()">
        <td>{{ z.zoneId }}</td>
        <td class="fw-semibold">{{ z.name }}</td>
        <td class="text-muted">{{ z.description }}</td>
        <td>{{ z.accessPointCount ?? '—' }}</td>
        <td>{{ z.activeRuleCount ?? '—' }}</td>
        <td class="text-end">
          <button class="btn btn-sm btn-outline-danger" (click)="remove(z)">{{ 'admin.delete' | translate }}</button>
        </td>
      </tr>
    </tbody>
  </table>
  `
})
export class AllZonesComponent {
  private api = inject(AdminApi);
  private lang = inject(LanguageService);

  zones = signal<Zone[]>([]);
  creating = signal(false);
  newName = '';
  newDesc = '';

  sorted = computed(() => {
    const collator = this.lang.collator();
    return [...this.zones()].sort((a, b) => collator.compare(a.name, b.name));
  });

  constructor() { this.reload(); }

  reload(): void { this.api.zones().subscribe(z => this.zones.set(z)); }

  create(): void {
    if (!this.newName.trim()) return;
    this.api.createZone(this.newName.trim(), this.newDesc.trim()).subscribe(() => {
      this.newName = ''; this.newDesc = ''; this.creating.set(false); this.reload();
    });
  }

  remove(z: Zone): void {
    if (!confirm(`${z.name}?`)) return;
    this.api.deleteZone(z.zoneId).subscribe(() => this.reload());
  }
}
