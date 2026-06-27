import { Routes } from '@angular/router';
import { authGuard, adminGuard } from './core/auth/auth.guard';

export const routes: Routes = [
  { path: '', pathMatch: 'full', redirectTo: 'app/zones' },

  {
    path: 'auth',
    children: [
      { path: 'login', loadComponent: () => import('./features/auth/login.component').then(m => m.LoginComponent) },
      { path: 'register', loadComponent: () => import('./features/auth/register.component').then(m => m.RegisterComponent) },
      { path: '', pathMatch: 'full', redirectTo: 'login' }
    ]
  },

  {
    path: 'app',
    canActivate: [authGuard],
    loadComponent: () => import('./features/user/user-layout.component').then(m => m.UserLayoutComponent),
    children: [
      { path: 'zones', loadComponent: () => import('./features/user/zones.component').then(m => m.ZonesComponent) },
      { path: 'zones/:id', loadComponent: () => import('./features/user/zone-detail.component').then(m => m.ZoneDetailComponent) },
      { path: 'profile', loadComponent: () => import('./features/user/profile.component').then(m => m.ProfileComponent) },
      { path: '', pathMatch: 'full', redirectTo: 'zones' }
    ]
  },

  {
    path: 'admin',
    canActivate: [authGuard, adminGuard],
    loadComponent: () => import('./features/admin/admin-layout.component').then(m => m.AdminLayoutComponent),
    children: [
      { path: 'users', loadComponent: () => import('./features/admin/users-list.component').then(m => m.UsersListComponent) },
      { path: 'zones', loadComponent: () => import('./features/admin/all-zones.component').then(m => m.AllZonesComponent) },
      { path: 'events', loadComponent: () => import('./features/admin/events.component').then(m => m.EventsComponent) },
      { path: 'backup', loadComponent: () => import('./features/admin/backup.component').then(m => m.BackupComponent) },
      { path: '', pathMatch: 'full', redirectTo: 'users' }
    ]
  },

  { path: '**', redirectTo: 'app/zones' }
];
