import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { ZonesApi } from '../../core/api/zones.api';
import { Zone } from '../../core/models/models';

@Component({
  selector: 'app-zones',
  standalone: true,
  imports: [CommonModule, RouterLink, TranslateModule],
  template: `
  <h3 class="mb-3">{{ 'zones.title' | translate }}</h3>
  <div *ngIf="loading()" class="text-center py-5"><div class="spinner-border text-pk"></div></div>
  <div *ngIf="!loading() && zones().length === 0" class="text-muted">{{ 'zones.empty' | translate }}</div>
  <div class="row g-3">
    <div class="col-md-4" *ngFor="let z of zones()">
      <div class="card h-100 shadow-sm">
        <div class="card-body">
          <h5 class="card-title">{{ z.name }}</h5>
          <p class="card-text text-muted small">{{ z.description }}</p>
          <div class="d-flex justify-content-between">
            <a class="btn btn-sm btn-pk" [routerLink]="['/app/zones', z.zoneId]">{{ 'zones.request' | translate }}</a>
            <a class="btn btn-sm btn-outline-danger" [routerLink]="['/app/zones', z.zoneId]">{{ 'zones.details' | translate }}</a>
          </div>
        </div>
      </div>
    </div>
  </div>
  `
})
export class ZonesComponent {
  private api = inject(ZonesApi);
  zones = signal<Zone[]>([]);
  loading = signal(true);

  constructor() {
    this.api.list(50).subscribe({
      next: (z) => { this.zones.set(z); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }
}
