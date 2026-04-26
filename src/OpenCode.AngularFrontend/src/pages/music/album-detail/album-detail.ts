import { Component, Input, Output, EventEmitter } from '@angular/core';
import { HasRoleDirective } from '../../../auth/has-role.directive';
import type { Album } from '../../../api/types';

@Component({
  selector: 'app-album-detail',
  standalone: true,
  imports: [HasRoleDirective],
  templateUrl: './album-detail.html',
  styleUrl: './album-detail.css',
})
export class AlbumDetailComponent {
  @Input({ required: true }) album!: Album;
  @Output() createTrack = new EventEmitter<void>();
  @Output() edit = new EventEmitter<void>();
  @Output() delete = new EventEmitter<void>();
  @Output() back = new EventEmitter<void>();

  formatDuration(s?: number): string {
    if (!s) return '—';
    const m = Math.floor(s / 60);
    const sec = s % 60;
    return `${m}:${String(sec).padStart(2, '0')}`;
  }
}
