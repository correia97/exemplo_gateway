import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', loadComponent: () => import('../pages/dashboard/dashboard').then(m => m.DashboardComponent) },
  { path: 'login', loadComponent: () => import('../pages/login/login').then(m => m.LoginComponent) },
  { path: 'callback', loadComponent: () => import('../pages/callback/callback').then(m => m.CallbackComponent) },
  { path: 'dragonball', loadChildren: () => import('../pages/dragonball/dragonball.routes').then(m => m.routes) },
  { path: 'music', loadChildren: () => import('../pages/music/music.routes').then(m => m.routes) },
  { path: '**', redirectTo: '', pathMatch: 'full' },
];
