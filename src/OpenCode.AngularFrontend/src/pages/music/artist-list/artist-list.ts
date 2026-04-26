import { Component, Output, EventEmitter, OnInit, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MusicService } from '../../../api/music.service';
import { HasRoleDirective } from '../../../auth/has-role.directive';
import { DataTableComponent } from '../../../shared/components/data-table/data-table';
import { PaginationComponent } from '../../../shared/components/pagination/pagination';
import { EmptyStateComponent } from '../../../shared/components/empty-state/empty-state';
import type { Artist } from '../../../api/types';

@Component({
  selector: 'app-artist-list',
  standalone: true,
  imports: [FormsModule, DataTableComponent, PaginationComponent, EmptyStateComponent, HasRoleDirective],
  templateUrl: './artist-list.html',
  styleUrl: './artist-list.css',
})
export class ArtistListComponent implements OnInit {
  private musicService = inject(MusicService);

  @Output() selectArtist = new EventEmitter<number>();
  @Output() createArtist = new EventEmitter<void>();

  artists: Artist[] = [];
  page = 1;
  totalPages = 1;
  totalCount = 0;
  isLoading = true;
  search = '';
  pageSize = 10;

  columns = [
    { key: 'name', label: 'Name' },
    { key: 'genre', label: 'Genre' },
    { key: 'albums', label: 'Albums', render: (a: Artist) => String(a.albums?.length ?? 0) },
  ];

  keyExtractor = (a: Artist) => a.id;

  ngOnInit(): void { this.fetchArtists(); }

  fetchArtists(): void {
    this.isLoading = true;
    this.musicService.getArtists({ page: this.page, pageSize: this.pageSize, name: this.search || undefined }).subscribe({
      next: (r) => { this.artists = r.items; this.totalPages = r.totalPages; this.totalCount = r.totalCount; this.isLoading = false; },
      error: () => { this.artists = []; this.isLoading = false; },
    });
  }

  onSearchChange(): void { this.page = 1; this.fetchArtists(); }
  onPageChange(p: number): void { this.page = p; this.fetchArtists(); }
}
