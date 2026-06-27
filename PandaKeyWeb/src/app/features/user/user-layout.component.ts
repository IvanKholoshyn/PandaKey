import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { TranslateModule } from '@ngx-translate/core';
import { AuthService } from '../../core/auth/auth.service';
import { LanguageSwitcherComponent } from '../../shared/language-switcher/language-switcher.component';

@Component({
  selector: 'app-user-layout',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink, RouterLinkActive, TranslateModule, LanguageSwitcherComponent],
  template: `
  <nav class="navbar navbar-dark navbar-user px-3">
    <div class="container-fluid">
      <div class="d-flex align-items-center gap-3">
        <span class="navbar-brand mb-0 fw-bold">{{ 'app.title' | translate }}</span>
        <a class="nav-link text-light" routerLink="/app/zones" routerLinkActive="fw-bold">{{ 'zones.title' | translate }}</a>
        <a class="nav-link text-light" routerLink="/app/profile" routerLinkActive="fw-bold">{{ 'profile.title' | translate }}</a>
      </div>
      <div class="d-flex align-items-center gap-3">
        <app-language-switcher />
        <span class="text-light small" *ngIf="auth.user() as u">{{ u.fullName }}</span>
        <a class="btn btn-sm btn-pk" *ngIf="auth.isAdmin()" routerLink="/admin/users">{{ 'app.admin' | translate }}</a>
        <button class="btn btn-sm btn-outline-light" (click)="logout()">{{ 'app.logout' | translate }}</button>
      </div>
    </div>
  </nav>
  <div class="container py-4">
    <router-outlet />
  </div>
  `
})
export class UserLayoutComponent {
  auth = inject(AuthService);
  private router = inject(Router);
  logout(): void { this.auth.logout(); this.router.navigate(['/auth/login']); }
}
