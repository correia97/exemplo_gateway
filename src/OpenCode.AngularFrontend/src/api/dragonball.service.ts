import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { API_BASE_URL } from './client.service';
import type { PaginatedResponse, Character, CharacterFilters, CharacterCreatePayload } from './types';

@Injectable({ providedIn: 'root' })
export class DragonballService {
  private http = inject(HttpClient);
  private baseUrl = `${API_BASE_URL}dragonball`;

  getCharacters(filters: CharacterFilters = {}): Observable<PaginatedResponse<Character>> {
    let params = new HttpParams();
    if (filters.page) params = params.set('page', filters.page);
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize);
    if (filters.name) params = params.set('name', filters.name);
    if (filters.race) params = params.set('race', filters.race);
    if (filters.minKi !== undefined) params = params.set('minKi', filters.minKi);
    if (filters.maxKi !== undefined) params = params.set('maxKi', filters.maxKi);
    return this.http.get<PaginatedResponse<Character>>(`${this.baseUrl}/characters`, { params });
  }

  getCharacter(id: number): Observable<Character> {
    return this.http.get<Character>(`${this.baseUrl}/characters/${id}`);
  }

  createCharacter(data: CharacterCreatePayload): Observable<Character> {
    return this.http.post<Character>(`${this.baseUrl}/characters`, data);
  }

  updateCharacter(id: number, data: Partial<CharacterCreatePayload>): Observable<Character> {
    return this.http.put<Character>(`${this.baseUrl}/characters/${id}`, data);
  }

  deleteCharacter(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/characters/${id}`);
  }
}
