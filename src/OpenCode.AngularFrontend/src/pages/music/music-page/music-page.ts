import { Component, inject } from '@angular/core';
import { AuthService } from '../../../auth/auth.service';
import { MusicService } from '../../../api/music.service';
import { ArtistListComponent } from '../artist-list/artist-list';
import { ArtistDetailComponent } from '../artist-detail/artist-detail';
import { AlbumDetailComponent } from '../album-detail/album-detail';
import { MusicFormComponent } from '../music-form/music-form';
import { ErrorDisplayComponent } from '../../../shared/components/error-display/error-display';
import { ToastService } from '../../../shared/services/toast.service';
import type { Artist, Album, Track, ArtistCreatePayload, AlbumCreatePayload, TrackCreatePayload } from '../../../api/types';

export type MusicView = 'artist-list' | 'artist-detail' | 'album-detail' | 'create-artist' | 'edit-artist' | 'create-album' | 'edit-album' | 'create-track' | 'edit-track';

@Component({
  selector: 'app-music-page',
  standalone: true,
  imports: [ArtistListComponent, ArtistDetailComponent, AlbumDetailComponent, MusicFormComponent, ErrorDisplayComponent],
  templateUrl: './music-page.html',
  styleUrl: './music-page.css',
})
export class MusicPageComponent {
  private musicService = inject(MusicService);
  private auth = inject(AuthService);
  private toast = inject(ToastService);

  view: MusicView = 'artist-list';
  selectedArtist: Artist | null = null;
  selectedAlbum: Album | null = null;
  isAuthenticated$ = this.auth.isAuthenticated$;

  showArtistList(): void { this.view = 'artist-list'; this.selectedArtist = null; this.selectedAlbum = null; }

  showArtistDetail(id: number): void {
    this.musicService.getArtist(id).subscribe({
      next: (a) => { this.selectedArtist = a; this.selectedAlbum = null; this.view = 'artist-detail'; },
      error: (err) => this.toast.showError(err.message || 'Failed to load artist'),
    });
  }

  showAlbumDetail(id: number): void {
    this.musicService.getAlbum(id).subscribe({
      next: (a) => { this.selectedAlbum = a; this.view = 'album-detail'; },
      error: (err) => this.toast.showError(err.message || 'Failed to load album'),
    });
  }

  goBack(): void {
    if (this.view === 'album-detail' || this.view.startsWith('create-track') || this.view.startsWith('edit-track')) {
      this.view = 'artist-detail';
    } else {
      this.view = 'artist-list';
    }
  }

  handleCreateArtist(data: ArtistCreatePayload): void {
    this.musicService.createArtist(data).subscribe({ next: () => this.showArtistList(), error: (err) => this.toast.showError(err.message) });
  }
  handleUpdateArtist(id: number, data: Partial<ArtistCreatePayload>): void {
    this.musicService.updateArtist(id, data).subscribe({ next: () => this.showArtistDetail(id), error: (err) => this.toast.showError(err.message) });
  }
  handleDeleteArtist(id: number): void {
    if (!confirm('Delete this artist and all albums?')) return;
    this.musicService.deleteArtist(id).subscribe({ next: () => this.showArtistList(), error: (err) => this.toast.showError(err.message) });
  }

  handleCreateAlbum(data: AlbumCreatePayload): void {
    this.musicService.createAlbum(data).subscribe({
      next: () => { if (this.selectedArtist) this.showArtistDetail(this.selectedArtist.id); },
      error: (err) => this.toast.showError(err.message),
    });
  }
  handleUpdateAlbum(id: number, data: Partial<AlbumCreatePayload>): void {
    this.musicService.updateAlbum(id, data).subscribe({ next: () => this.showAlbumDetail(id), error: (err) => this.toast.showError(err.message) });
  }
  handleDeleteAlbum(id: number): void {
    if (!confirm('Delete this album and all tracks?')) return;
    this.musicService.deleteAlbum(id).subscribe({
      next: () => { if (this.selectedArtist) this.showArtistDetail(this.selectedArtist.id); },
      error: (err) => this.toast.showError(err.message),
    });
  }

  handleCreateTrack(data: TrackCreatePayload): void {
    this.musicService.createTrack(data).subscribe({
      next: () => { if (this.selectedAlbum) this.showAlbumDetail(this.selectedAlbum.id); },
      error: (err) => this.toast.showError(err.message),
    });
  }
  handleUpdateTrack(id: number, data: Partial<TrackCreatePayload>): void {
    this.musicService.updateTrack(id, data).subscribe({
      next: () => { if (this.selectedAlbum) this.showAlbumDetail(this.selectedAlbum.id); },
      error: (err) => this.toast.showError(err.message),
    });
  }
  handleDeleteTrack(id: number): void {
    if (!confirm('Delete this track?')) return;
    this.musicService.deleteTrack(id).subscribe({
      next: () => { if (this.selectedAlbum) this.showAlbumDetail(this.selectedAlbum.id); },
      error: (err) => this.toast.showError(err.message),
    });
  }
}
