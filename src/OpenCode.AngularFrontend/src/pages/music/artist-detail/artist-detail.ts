import { Component, Input, Output, EventEmitter } from '@angular/core';
import { HasRoleDirective } from '../../../auth/has-role.directive';
import type { Artist } from '../../../api/types';

@Component({
  selector: 'app-artist-detail',
  standalone: true,
  imports: [HasRoleDirective],
  templateUrl: './artist-detail.html',
  styleUrl: './artist-detail.css',
})
export class ArtistDetailComponent {
  @Input({ required: true }) artist!: Artist;
  @Output() selectAlbum = new EventEmitter<number>();
  @Output() createAlbum = new EventEmitter<void>();
  @Output() edit = new EventEmitter<void>();
  @Output() delete = new EventEmitter<void>();
  @Output() back = new EventEmitter<void>();
}
