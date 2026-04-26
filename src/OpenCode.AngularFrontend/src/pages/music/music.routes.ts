import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./music-page/music-page').then(m => m.MusicPageComponent),
  },
];
