import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { MUSIC_API_URL } from './env';
import type { PaginatedResponse, Artist, Album, Track, ArtistCreatePayload, AlbumCreatePayload, TrackCreatePayload, MusicFilters } from './types';

@Injectable({ providedIn: 'root' })
export class MusicService {
  private http = inject(HttpClient);

  // --- Artists ---
  getArtists(filters: MusicFilters = {}): Observable<PaginatedResponse<Artist>> {
    let params = new HttpParams();
    if (filters.page) params = params.set('page', filters.page);
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize);
    if (filters.name) params = params.set('name', filters.name);
    return this.http.get<PaginatedResponse<Artist>>(`${MUSIC_API_URL}/api/v1/artists`, { params });
  }

  getArtist(id: number): Observable<Artist> {
    return this.http.get<Artist>(`${MUSIC_API_URL}/api/v1/artists/${id}`);
  }

  createArtist(data: ArtistCreatePayload): Observable<Artist> {
    return this.http.post<Artist>(`${MUSIC_API_URL}/api/v1/artists`, data);
  }

  updateArtist(id: number, data: Partial<ArtistCreatePayload>): Observable<Artist> {
    return this.http.put<Artist>(`${MUSIC_API_URL}/api/v1/artists/${id}`, data);
  }

  deleteArtist(id: number): Observable<void> {
    return this.http.delete<void>(`${MUSIC_API_URL}/api/v1/artists/${id}`);
  }

  // --- Albums ---
  getAlbums(filters: MusicFilters = {}): Observable<PaginatedResponse<Album>> {
    let params = new HttpParams();
    if (filters.page) params = params.set('page', filters.page);
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize);
    if (filters.title) params = params.set('title', filters.title);
    return this.http.get<PaginatedResponse<Album>>(`${MUSIC_API_URL}/api/v1/albums`, { params });
  }

  getAlbum(id: number): Observable<Album> {
    return this.http.get<Album>(`${MUSIC_API_URL}/api/v1/albums/${id}`);
  }

  createAlbum(data: AlbumCreatePayload): Observable<Album> {
    return this.http.post<Album>(`${MUSIC_API_URL}/api/v1/albums`, data);
  }

  updateAlbum(id: number, data: Partial<AlbumCreatePayload>): Observable<Album> {
    return this.http.put<Album>(`${MUSIC_API_URL}/api/v1/albums/${id}`, data);
  }

  deleteAlbum(id: number): Observable<void> {
    return this.http.delete<void>(`${MUSIC_API_URL}/api/v1/albums/${id}`);
  }

  // --- Tracks ---
  getTracks(filters: MusicFilters = {}): Observable<PaginatedResponse<Track>> {
    let params = new HttpParams();
    if (filters.page) params = params.set('page', filters.page);
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize);
    if (filters.title) params = params.set('title', filters.title);
    return this.http.get<PaginatedResponse<Track>>(`${MUSIC_API_URL}/api/v1/tracks`, { params });
  }

  getTrack(id: number): Observable<Track> {
    return this.http.get<Track>(`${MUSIC_API_URL}/api/v1/tracks/${id}`);
  }

  createTrack(data: TrackCreatePayload): Observable<Track> {
    return this.http.post<Track>(`${MUSIC_API_URL}/api/v1/tracks`, data);
  }

  updateTrack(id: number, data: Partial<TrackCreatePayload>): Observable<Track> {
    return this.http.put<Track>(`${MUSIC_API_URL}/api/v1/tracks/${id}`, data);
  }

  deleteTrack(id: number): Observable<void> {
    return this.http.delete<void>(`${MUSIC_API_URL}/api/v1/tracks/${id}`);
  }
}
