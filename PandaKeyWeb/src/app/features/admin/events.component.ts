import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { EventsApi } from '../../core/api/events.api';
import { LanguageService } from '../../core/i18n/language.service';
import { AccessEvent } from '../../core/models/models';

@Component({
  selector: 'app-admin-events',
  standalone: true,
  imports: [CommonModule, TranslateModule, DatePipe],
  template: `
  <h3 class="mb-3">{{ 'events.title' | translate }}</h3>
  <table class="table align-middle">
    <thead>
      <tr>
        <th>{{ 'events.time' | translate }}</th>
        <th>{{ 'events.point' | translate }}</th>
        <th>UserId</th>
        <th>{{ 'events.result' | translate }}</th>
        <th>{{ 'events.reason' | translate }}</th>
      </tr>
    </thead>
    <tbody>
      <tr *ngFor="let e of events()">
        <td class="small">{{ e.eventTime | date:'short':'':locale() }}</td>
        <td>#{{ e.accessPointId }}</td>
        <td>{{ e.userId ?? '—' }}</td>
        <td><span class="badge" [class.bg-success]="isGranted(e)" [class.bg-danger]="!isGranted(e)">{{ e.result }}</span></td>
        <td class="small text-muted">{{ e.reason }}</td>
      </tr>
    </tbody>
  </table>
  <div *ngIf="events().length === 0" class="text-muted">{{ 'events.empty' | translate }}</div>
  `
})
export class EventsComponent {
  private api = inject(EventsApi);
  private lang = inject(LanguageService);
  events = signal<AccessEvent[]>([]);
  locale = computed(() => this.lang.current());

  constructor() { this.api.latest(100).subscribe(e => this.events.set(e)); }
  isGranted(e: AccessEvent): boolean { return (e.result || '').toUpperCase() === 'GRANTED'; }
}
