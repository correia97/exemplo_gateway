import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import type { Artist, Album, Track, ArtistCreatePayload, AlbumCreatePayload, TrackCreatePayload } from '../../../api/types';

export type FormMode = 'create-artist' | 'edit-artist' | 'create-album' | 'edit-album' | 'create-track' | 'edit-track';

@Component({
  selector: 'app-music-form',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './music-form.html',
  styleUrl: './music-form.css',
})
export class MusicFormComponent implements OnInit {
  @Input() mode: FormMode = 'create-artist';
  @Input() initial?: Partial<Artist | Album | Track> & { artistId?: number; albumId?: number };
  @Output() save = new EventEmitter<ArtistCreatePayload | AlbumCreatePayload | TrackCreatePayload>();
  @Output() cancel = new EventEmitter<void>();

  name = '';
  title = '';
  genre = '';
  biography = '';
  releaseYear: number | undefined;
  duration: number | undefined;
  trackNumber: number | undefined;
  lyrics = '';
  submitting = false;

  get isArtistForm(): boolean { return this.mode === 'create-artist' || this.mode === 'edit-artist'; }
  get isAlbumForm(): boolean { return this.mode === 'create-album' || this.mode === 'edit-album'; }
  get isTrackForm(): boolean { return this.mode === 'create-track' || this.mode === 'edit-track'; }
  get isCreate(): boolean { return this.mode.startsWith('create-'); }
  get titleText(): string {
    if (this.isArtistForm) return 'Artist';
    if (this.isAlbumForm) return 'Album';
    return 'Track';
  }

  ngOnInit(): void {
    if (this.initial) {
      if ('name' in this.initial && this.initial.name) this.name = this.initial.name;
      if ('title' in this.initial && this.initial.title) this.title = this.initial.title;
      if ('genre' in this.initial && this.initial.genre) this.genre = this.initial.genre;
      if ('biography' in this.initial && this.initial.biography) this.biography = this.initial.biography;
      if ('releaseYear' in this.initial) this.releaseYear = (this.initial as Album).releaseYear;
      if ('duration' in this.initial) this.duration = (this.initial as Track).duration;
      if ('trackNumber' in this.initial) this.trackNumber = (this.initial as Track).trackNumber;
      if ('lyrics' in this.initial && this.initial.lyrics) this.lyrics = this.initial.lyrics;
    }
  }

  onSubmit(): void {
    this.submitting = true;
    try {
      if (this.isArtistForm) {
        this.save.emit({ name: this.name.trim(), genre: this.genre.trim() || undefined, biography: this.biography.trim() || undefined });
      } else if (this.isAlbumForm) {
        this.save.emit({
          title: this.title.trim(),
          genre: this.genre.trim() || undefined,
          releaseYear: this.releaseYear,
          artistId: this.initial?.artistId ?? 0,
        });
      } else if (this.isTrackForm) {
        this.save.emit({
          title: this.title.trim(),
          duration: this.duration,
          trackNumber: this.trackNumber,
          lyrics: this.lyrics.trim() || undefined,
          albumId: this.initial?.albumId ?? 0,
        });
      }
    } finally {
      this.submitting = false;
    }
  }
}
