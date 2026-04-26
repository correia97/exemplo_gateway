import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { DRAGONBALL_API_URL } from './env';
import type { PaginatedResponse, Character, CharacterFilters, CharacterCreatePayload } from './types';

@Injectable({ providedIn: 'root' })
export class DragonballService {
  private http = inject(HttpClient);

  getCharacters(filters: CharacterFilters = {}): Observable<PaginatedResponse<Character>> {
    let params = new HttpParams();
    if (filters.page) params = params.set('page', filters.page);
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize);
    if (filters.name) params = params.set('name', filters.name);
    if (filters.race) params = params.set('race', filters.race);
    if (filters.minKi !== undefined) params = params.set('minKi', filters.minKi);
    if (filters.maxKi !== undefined) params = params.set('maxKi', filters.maxKi);
    return this.http.get<PaginatedResponse<Character>>(`${DRAGONBALL_API_URL}/api/characters`, { params });
  }

  getCharacter(id: number): Observable<Character> {
    return this.http.get<Character>(`${DRAGONBALL_API_URL}/api/characters/${id}`);
  }

  createCharacter(data: CharacterCreatePayload): Observable<Character> {
    return this.http.post<Character>(`${DRAGONBALL_API_URL}/api/characters`, data);
  }

  updateCharacter(id: number, data: Partial<CharacterCreatePayload>): Observable<Character> {
    return this.http.put<Character>(`${DRAGONBALL_API_URL}/api/characters/${id}`, data);
  }

  deleteCharacter(id: number): Observable<void> {
    return this.http.delete<void>(`${DRAGONBALL_API_URL}/api/characters/${id}`);
  }
}
