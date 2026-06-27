import { Component, computed, inject } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../core/auth/auth.service';
import { LanguageService } from '../../core/i18n/language.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, TranslateModule, DatePipe],
  template: `
  <h3 class="mb-3">{{ 'profile.title' | translate }}</h3>
  <div class="card shadow-sm" *ngIf="auth.user() as u" style="max-width: 560px;">
    <div class="card-body">
      <h5 class="mb-1">{{ u.fullName }}</h5>
      <div class="text-muted mb-3">{{ u.email }}</div>
      <table class="table table-sm">
        <tbody>
          <tr><th>UserId</th><td>{{ u.userId }}</td></tr>
          <tr><th>{{ 'profile.department' | translate }}</th><td>{{ u.departmentId ?? '—' }}</td></tr>
          <tr><th>{{ 'auth.phone' | translate }}</th><td>{{ u.phone ?? '—' }}</td></tr>
          <tr><th>{{ 'admin.roles' | translate }}</th><td><span class="badge bg-secondary">{{ u.role }}</span></td></tr>
          <tr><th>{{ 'profile.status' | translate }}</th>
              <td>{{ (u.isActive ? 'profile.active' : 'profile.inactive') | translate }}</td></tr>
          <tr *ngIf="u.createdAt"><th>{{ 'profile.memberSince' | translate }}</th>
              <td>{{ u.createdAt | date:'mediumDate':'':locale() }}</td></tr>
        </tbody>
      </table>
    </div>
  </div>
  `
})
export class ProfileComponent {
  auth = inject(AuthService);
  private lang = inject(LanguageService);
  locale = computed(() => this.lang.current());
}
