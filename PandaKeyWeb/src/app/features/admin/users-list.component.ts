import { Component, computed, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslateModule } from '@ngx-translate/core';
import { AdminApi } from '../../core/api/admin.api';
import { LanguageService } from '../../core/i18n/language.service';
import { Role, User } from '../../core/models/models';

@Component({
  selector: 'app-users-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslateModule],
  template: `
  <h3 class="mb-3">{{ 'admin.users' | translate }}</h3>
  <input class="form-control mb-3" style="max-width: 320px;"
         [placeholder]="'admin.search' | translate" [(ngModel)]="query" (ngModelChange)="query.set($event)" />
  <table class="table align-middle">
    <thead>
      <tr>
        <th>{{ 'auth.email' | translate }}</th>
        <th>{{ 'admin.name' | translate }}</th>
        <th>{{ 'admin.roles' | translate }}</th>
        <th>{{ 'profile.status' | translate }}</th>
        <th class="text-end">{{ 'admin.actions' | translate }}</th>
      </tr>
    </thead>
    <tbody>
      <tr *ngFor="let u of filtered()">
        <td>{{ u.email }}</td>
        <td>{{ u.fullName }}</td>
        <td>
          <span class="badge me-1" [class.bg-warning]="u.role === 'admin'" [class.text-dark]="u.role === 'admin'"
                [class.bg-secondary]="u.role !== 'admin'">{{ u.role }}</span>
        </td>
        <td>{{ (u.isActive ? 'profile.active' : 'profile.inactive') | translate }}</td>
        <td class="text-end">
          <button class="btn btn-sm me-2"
                  [class.btn-outline-warning]="u.role === 'admin'" [class.btn-warning]="u.role !== 'admin'"
                  (click)="toggleAdmin(u)">
            {{ (u.role === 'admin' ? 'admin.removeAdmin' : 'admin.makeAdmin') | translate }}
          </button>
          <button class="btn btn-sm btn-outline-danger" (click)="remove(u)">{{ 'admin.delete' | translate }}</button>
        </td>
      </tr>
    </tbody>
  </table>
  `
})
export class UsersListComponent {
  private api = inject(AdminApi);
  private lang = inject(LanguageService);

  users = signal<User[]>([]);
  query = signal('');

  filtered = computed(() => {
    const q = this.query().trim().toLowerCase();
    const list = this.users().filter(u =>
      !q || u.email.toLowerCase().includes(q) || u.fullName.toLowerCase().includes(q));
    const collator = this.lang.collator();
    return [...list].sort((a, b) => collator.compare(a.fullName, b.fullName));
  });

  constructor() { this.reload(); }

  reload(): void { this.api.users().subscribe(u => this.users.set(u)); }

  toggleAdmin(u: User): void {
    const role: Role = u.role === 'admin' ? 'user' : 'admin';
    this.api.setRole(u.userId, role).subscribe(() => this.reload());
  }

  remove(u: User): void {
    if (!confirm(`${u.fullName}?`)) return;
    this.api.deleteUser(u.userId).subscribe(() => this.reload());
  }
}
