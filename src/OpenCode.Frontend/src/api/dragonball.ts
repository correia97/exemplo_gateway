import apiFetch from './client'
import type { PaginatedResponse, Character, CharacterFilters, CharacterCreatePayload } from './types'

export async function getCharacters(filters: CharacterFilters = {}): Promise<PaginatedResponse<Character>> {
  const params = new URLSearchParams()
  if (filters.page) params.set('page', String(filters.page))
  if (filters.pageSize) params.set('pageSize', String(filters.pageSize))
  if (filters.name) params.set('name', filters.name)
  if (filters.race) params.set('race', filters.race)
  if (filters.minKi !== undefined) params.set('minKi', String(filters.minKi))
  if (filters.maxKi !== undefined) params.set('maxKi', String(filters.maxKi))
  const qs = params.toString()
  return apiFetch<PaginatedResponse<Character>>(`/api/dragonball/characters${qs ? `?${qs}` : ''}`)
}

export async function getCharacter(id: number): Promise<Character> {
  return apiFetch<Character>(`/api/dragonball/characters/${id}`)
}

export async function createCharacter(data: CharacterCreatePayload): Promise<Character> {
  return apiFetch<Character>(`/api/dragonball/characters`, {
    method: 'POST',
    body: JSON.stringify(data),
  })
}

export async function updateCharacter(id: number, data: Partial<CharacterCreatePayload>): Promise<Character> {
  return apiFetch<Character>(`/api/dragonball/characters/${id}`, {
    method: 'PUT',
    body: JSON.stringify(data),
  })
}

export async function deleteCharacter(id: number): Promise<void> {
  return apiFetch<void>(`/api/dragonball/characters/${id}`, { method: 'DELETE' })
}
