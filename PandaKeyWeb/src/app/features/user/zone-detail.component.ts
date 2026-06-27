import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AccessApi } from '../../core/api/access.api';
import { EventsApi } from '../../core/api/events.api';
import { AuthService } from '../../core/auth/auth.service';
import { LanguageService } from '../../core/i18n/language.service';
import { AccessDecision, AccessEvent } from '../../core/models/models';

@Component({
  selector: 'app-zone-detail',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink, TranslateModule, DatePipe],
  template: `
  <a routerLink="/app/zones" class="text-pk small">{{ 'zones.back' | translate }}</a>
  <h3 class="mb-3">{{ 'zones.title' | translate }} #{{ zoneId }}</h3>
  <div class="row g-3">
    <div class="col-lg-6">
      <div class="card shadow-sm">
        <div class="card-body">
          <label class="form-label fw-semibold">{{ 'zones.accessPoint' | translate }}</label>
          <div class="input-group">
            <input class="form-control" type="number" [(ngModel)]="accessPointId" min="1" />
            <button class="btn btn-pk" [disabled]="loading()" (click)="request()">
              <span *ngIf="loading()" class="spinner-border spinner-border-sm me-1"></span>
              {{ 'zones.request' | translate }}
            </button>
          </div>

          <div *ngIf="decision() as d" class="alert mt-3 mb-0"
               [class.alert-success]="d.granted" [class.alert-danger]="!d.granted">
            <strong>{{ (d.granted ? 'zones.decisionGranted' : 'zones.decisionDenied') | translate }}</strong>
            <div class="small">{{ d.reason }}</div>
            <div class="small text-muted" *ngIf="d.utc">{{ d.utc | date:'short':'':locale() }}</div>
            <div class="small text-muted mt-1">{{ 'zones.eventLogged' | translate }}</div>
          </div>
        </div>
      </div>
    </div>

    <div class="col-lg-6">
      <div class="card shadow-sm">
        <div class="card-body">
          <h6>{{ 'zones.recent' | translate }}</h6>
          <table class="table table-sm align-middle">
            <tbody>
              <tr *ngFor="let e of pointEvents()">
                <td class="small">{{ e.eventTime | date:'short':'':locale() }}</td>
                <td>
                  <span class="badge" [class.bg-success]="isGranted(e)" [class.bg-danger]="!isGranted(e)">{{ e.result }}</span>
                </td>
                <td class="small text-muted">{{ e.reason }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>
    </div>
  </div>
  `
})
export class ZoneDetailComponent {
  private route = inject(ActivatedRoute);
  private access = inject(AccessApi);
  private events = inject(EventsApi);
  private auth = inject(AuthService);
  private lang = inject(LanguageService);

  zoneId = Number(this.route.snapshot.paramMap.get('id') ?? 0);
  accessPointId = 1;
  loading = signal(false);
  decision = signal<AccessDecision | null>(null);
  pointEvents = signal<AccessEvent[]>([]);
  locale = computed(() => this.lang.current());

  constructor() {
    this.events.latest(50).subscribe((list) => this.pointEvents.set(list.slice(0, 6)));
  }

  isGranted(e: AccessEvent): boolean {
    return (e.result || '').toUpperCase() === 'GRANTED';
  }

  request(): void {
    const userId = this.auth.user()?.userId;
    if (!userId) return;
    this.loading.set(true);
    this.access.decide(userId, this.accessPointId).subscribe({
      next: (d) => {
        this.decision.set(d);
        this.loading.set(false);
        this.events.latest(50).subscribe((list) => this.pointEvents.set(list.slice(0, 6)));
      },
      error: () => this.loading.set(false)
    });
  }
}
