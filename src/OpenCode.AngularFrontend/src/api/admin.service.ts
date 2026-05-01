import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, forkJoin, map } from 'rxjs';

export interface AdminStats {
  characters: number;
  genres: number;
  artists: number;
  albums: number;
  tracks: number;
}

@Injectable({ providedIn: 'root' })
export class AdminService {
  private http = inject(HttpClient);

  /**
   * Fetch all entity counts by calling existing list endpoints with pageSize=1.
   * Reads totalCount from the paginated response. No dedicated stats endpoint.
   */
  fetchStats(): Observable<AdminStats> {
    const baseUrl = 'http://localhost:8000/api'; // through Kong gateway
    const pageSize1 = '?pageSize=1';

    const characters$ = this.http.get<any>(`${baseUrl}/characters${pageSize1}`)
      .pipe(map(r => r.totalCount ?? 0));
    const genres$ = this.http.get<any>(`${baseUrl}/genres${pageSize1}`)
      .pipe(map(r => r.totalCount ?? 0));
    const artists$ = this.http.get<any>(`${baseUrl}/artists${pageSize1}`)
      .pipe(map(r => r.totalCount ?? 0));
    const albums$ = this.http.get<any>(`${baseUrl}/albums${pageSize1}`)
      .pipe(map(r => r.totalCount ?? 0));
    const tracks$ = this.http.get<any>(`${baseUrl}/tracks${pageSize1}`)
      .pipe(map(r => r.totalCount ?? 0));

    return forkJoin({
      characters: characters$,
      genres: genres$,
      artists: artists$,
      albums: albums$,
      tracks: tracks$,
    });
  }
}
