import apiFetch from './client'
import type { PaginatedResponse, Artist, Album, Track, ArtistCreatePayload, AlbumCreatePayload, TrackCreatePayload, MusicFilters } from './types'

export async function getArtists(filters: MusicFilters = {}): Promise<PaginatedResponse<Artist>> {
  const params = new URLSearchParams()
  if (filters.page) params.set('page', String(filters.page))
  if (filters.pageSize) params.set('pageSize', String(filters.pageSize))
  if (filters.name) params.set('name', filters.name)
  const qs = params.toString()
  return apiFetch<PaginatedResponse<Artist>>(`/api/music/artists${qs ? `?${qs}` : ''}`)
}

export async function getArtist(id: number): Promise<Artist> {
  return apiFetch<Artist>(`/api/music/artists/${id}`)
}

export async function createArtist(data: ArtistCreatePayload): Promise<Artist> {
  return apiFetch<Artist>(`/api/music/artists`, { method: 'POST', body: JSON.stringify(data) })
}

export async function updateArtist(id: number, data: Partial<ArtistCreatePayload>): Promise<Artist> {
  return apiFetch<Artist>(`/api/music/artists/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}

export async function deleteArtist(id: number): Promise<void> {
  return apiFetch<void>(`/api/music/artists/${id}`, { method: 'DELETE' })
}

export async function getAlbums(filters: MusicFilters = {}): Promise<PaginatedResponse<Album>> {
  const params = new URLSearchParams()
  if (filters.page) params.set('page', String(filters.page))
  if (filters.pageSize) params.set('pageSize', String(filters.pageSize))
  if (filters.title) params.set('title', filters.title)
  const qs = params.toString()
  return apiFetch<PaginatedResponse<Album>>(`/api/music/albums${qs ? `?${qs}` : ''}`)
}

export async function getAlbum(id: number): Promise<Album> {
  return apiFetch<Album>(`/api/music/albums/${id}`)
}

export async function createAlbum(data: AlbumCreatePayload): Promise<Album> {
  return apiFetch<Album>(`/api/music/albums`, { method: 'POST', body: JSON.stringify(data) })
}

export async function updateAlbum(id: number, data: Partial<AlbumCreatePayload>): Promise<Album> {
  return apiFetch<Album>(`/api/music/albums/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}

export async function deleteAlbum(id: number): Promise<void> {
  return apiFetch<void>(`/api/music/albums/${id}`, { method: 'DELETE' })
}

export async function getTracks(filters: MusicFilters = {}): Promise<PaginatedResponse<Track>> {
  const params = new URLSearchParams()
  if (filters.page) params.set('page', String(filters.page))
  if (filters.pageSize) params.set('pageSize', String(filters.pageSize))
  if (filters.title) params.set('title', filters.title)
  const qs = params.toString()
  return apiFetch<PaginatedResponse<Track>>(`/api/music/tracks${qs ? `?${qs}` : ''}`)
}

export async function getTrack(id: number): Promise<Track> {
  return apiFetch<Track>(`/api/music/tracks/${id}`)
}

export async function createTrack(data: TrackCreatePayload): Promise<Track> {
  return apiFetch<Track>(`/api/music/tracks`, { method: 'POST', body: JSON.stringify(data) })
}

export async function updateTrack(id: number, data: Partial<TrackCreatePayload>): Promise<Track> {
  return apiFetch<Track>(`/api/music/tracks/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}

export async function deleteTrack(id: number): Promise<void> {
  return apiFetch<void>(`/api/music/tracks/${id}`, { method: 'DELETE' })
}
