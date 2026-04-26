import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./dragonball-page/dragonball-page').then(m => m.DragonballPageComponent),
  },
];
