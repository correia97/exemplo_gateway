import { Component, inject } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { AdminService, AdminStats } from '../../../api/admin.service';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [AsyncPipe, RouterLink],
  templateUrl: './dashboard.component.html',
})
export class DashboardComponent {
  private adminService = inject(AdminService);
  stats$ = this.adminService.fetchStats();

  cards = [
    { label: 'Characters', key: 'characters', path: '/admin/characters', color: 'bg-blue-500' },
    { label: 'Genres', key: 'genres', path: '/admin/genres', color: 'bg-purple-500' },
    { label: 'Artists', key: 'artists', path: '/admin/artists', color: 'bg-indigo-500' },
    { label: 'Albums', key: 'albums', path: '/admin/albums', color: 'bg-pink-500' },
    { label: 'Tracks', key: 'tracks', path: '/admin/tracks', color: 'bg-teal-500' },
  ];
}
