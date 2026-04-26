import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './client.service';
import type { PaginatedResponse, Artist, Album, Track, ArtistCreatePayload, AlbumCreatePayload, TrackCreatePayload, MusicFilters } from './types';

@Injectable({ providedIn: 'root' })
export class MusicService {
  private http = inject(HttpClient);
  private baseUrl = `${API_BASE_URL}music`;

  // --- Artists ---
  getArtists(filters: MusicFilters = {}): Observable<PaginatedResponse<Artist>> {
    let params = new HttpParams();
    if (filters.page) params = params.set('page', filters.page);
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize);
    if (filters.name) params = params.set('name', filters.name);
    return this.http.get<PaginatedResponse<Artist>>(`${this.baseUrl}/artists`, { params });
  }

  getArtist(id: number): Observable<Artist> {
    return this.http.get<Artist>(`${this.baseUrl}/artists/${id}`);
  }

  createArtist(data: ArtistCreatePayload): Observable<Artist> {
    return this.http.post<Artist>(`${this.baseUrl}/artists`, data);
  }

  updateArtist(id: number, data: Partial<ArtistCreatePayload>): Observable<Artist> {
    return this.http.put<Artist>(`${this.baseUrl}/artists/${id}`, data);
  }

  deleteArtist(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/artists/${id}`);
  }

  // --- Albums ---
  getAlbums(filters: MusicFilters = {}): Observable<PaginatedResponse<Album>> {
    let params = new HttpParams();
    if (filters.page) params = params.set('page', filters.page);
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize);
    if (filters.title) params = params.set('title', filters.title);
    return this.http.get<PaginatedResponse<Album>>(`${this.baseUrl}/albums`, { params });
  }

  getAlbum(id: number): Observable<Album> {
    return this.http.get<Album>(`${this.baseUrl}/albums/${id}`);
  }

  createAlbum(data: AlbumCreatePayload): Observable<Album> {
    return this.http.post<Album>(`${this.baseUrl}/albums`, data);
  }

  updateAlbum(id: number, data: Partial<AlbumCreatePayload>): Observable<Album> {
    return this.http.put<Album>(`${this.baseUrl}/albums/${id}`, data);
  }

  deleteAlbum(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/albums/${id}`);
  }

  // --- Tracks ---
  getTracks(filters: MusicFilters = {}): Observable<PaginatedResponse<Track>> {
    let params = new HttpParams();
    if (filters.page) params = params.set('page', filters.page);
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize);
    if (filters.title) params = params.set('title', filters.title);
    return this.http.get<PaginatedResponse<Track>>(`${this.baseUrl}/tracks`, { params });
  }

  getTrack(id: number): Observable<Track> {
    return this.http.get<Track>(`${this.baseUrl}/tracks/${id}`);
  }

  createTrack(data: TrackCreatePayload): Observable<Track> {
    return this.http.post<Track>(`${this.baseUrl}/tracks`, data);
  }

  updateTrack(id: number, data: Partial<TrackCreatePayload>): Observable<Track> {
    return this.http.put<Track>(`${this.baseUrl}/tracks/${id}`, data);
  }

  deleteTrack(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/tracks/${id}`);
  }
}
