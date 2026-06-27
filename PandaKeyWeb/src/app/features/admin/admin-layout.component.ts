import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../core/auth/auth.service';
import { LanguageSwitcherComponent } from '../../shared/language-switcher/language-switcher.component';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, TranslateModule, LanguageSwitcherComponent],
  template: `
  <nav class="navbar navbar-admin px-3">
    <div class="container-fluid">
      <div class="d-flex align-items-center gap-3">
        <span class="navbar-brand mb-0 fw-bold text-white">{{ 'app.title' | translate }} · {{ 'app.admin' | translate }}</span>
        <a class="nav-link text-white" routerLink="/admin/users" routerLinkActive="fw-bold">{{ 'admin.users' | translate }}</a>
        <a class="nav-link text-white" routerLink="/admin/zones" routerLinkActive="fw-bold">{{ 'admin.allZones' | translate }}</a>
        <a class="nav-link text-white" routerLink="/admin/events" routerLinkActive="fw-bold">{{ 'admin.events' | translate }}</a>
        <a class="nav-link text-white" routerLink="/admin/backup" routerLinkActive="fw-bold">{{ 'admin.backup' | translate }}</a>
      </div>
      <div class="d-flex align-items-center gap-3">
        <app-language-switcher />
        <a class="btn btn-sm btn-light" routerLink="/app/zones">{{ 'app.userView' | translate }}</a>
        <span class="text-white small" *ngIf="auth.user() as u">{{ u.fullName }}</span>
        <button class="btn btn-sm btn-light" (click)="logout()">{{ 'app.logout' | translate }}</button>
      </div>
    </div>
  </nav>
  <div class="container-fluid py-4">
    <router-outlet />
  </div>
  `
})
export class AdminLayoutComponent {
  auth = inject(AuthService);
  private router = inject(Router);
  logout(): void { this.auth.logout(); this.router.navigate(['/auth/login']); }
}
