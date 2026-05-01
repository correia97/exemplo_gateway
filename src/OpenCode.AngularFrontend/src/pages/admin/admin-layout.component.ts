import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-admin-layout',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './admin-layout.component.html',
  styleUrl: './admin-layout.component.css',
})
export class AdminLayoutComponent {
  navItems: ({ label: string; path?: string; section?: boolean })[] = [
    { label: 'Dashboard', path: '/admin' },
    { label: 'Dragon Ball', section: true },
    { label: 'Characters', path: '/admin/characters' },
    { label: 'Music', section: true },
    { label: 'Genres', path: '/admin/genres' },
    { label: 'Artists', path: '/admin/artists' },
    { label: 'Albums', path: '/admin/albums' },
    { label: 'Tracks', path: '/admin/tracks' },
  ];
}
