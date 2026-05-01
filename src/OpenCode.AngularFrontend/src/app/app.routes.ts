import { Routes } from '@angular/router';
import { RoleGuard } from '../auth/role.guard';

export const routes: Routes = [
  { path: '', loadComponent: () => import('../pages/dashboard/dashboard').then(m => m.DashboardComponent) },
  { path: 'login', loadComponent: () => import('../pages/login/login').then(m => m.LoginComponent) },
  { path: 'callback', loadComponent: () => import('../pages/callback/callback').then(m => m.CallbackComponent) },
  { path: 'dragonball', loadChildren: () => import('../pages/dragonball/dragonball.routes').then(m => m.routes) },
  { path: 'music', loadChildren: () => import('../pages/music/music.routes').then(m => m.routes) },
  {
    path: 'admin',
    loadComponent: () => import('../pages/admin/admin-layout.component').then(m => m.AdminLayoutComponent),
    canActivate: [RoleGuard],
    data: { roles: ['editor'] },
    children: [
      { path: '', loadComponent: () => import('../pages/admin/dashboard/dashboard.component').then(m => m.DashboardComponent) },
      { path: 'characters', loadComponent: () => import('../pages/admin/characters/characters.component').then(m => m.CharactersComponent) },
      { path: 'genres', loadComponent: () => import('../pages/admin/genres/genres.component').then(m => m.GenresComponent) },
      { path: 'artists', loadComponent: () => import('../pages/admin/artists/artists.component').then(m => m.ArtistsComponent) },
      { path: 'albums', loadComponent: () => import('../pages/admin/albums/albums.component').then(m => m.AlbumsComponent) },
      { path: 'tracks', loadComponent: () => import('../pages/admin/tracks/tracks.component').then(m => m.TracksComponent) },
    ],
  },
  { path: '**', redirectTo: '', pathMatch: 'full' },
];
