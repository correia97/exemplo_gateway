import apiFetch, { MUSIC_API_URL } from './client'
import type { PaginatedResponse, Artist, Album, Track, Genre, ArtistCreatePayload, AlbumCreatePayload, TrackCreatePayload, GenreCreatePayload, MusicFilters, GenreFilters } from './types'

export async function getArtists(filters: MusicFilters = {}): Promise<PaginatedResponse<Artist>> {
  const params = new URLSearchParams()
  if (filters.page) params.set('page', String(filters.page))
  if (filters.pageSize) params.set('pageSize', String(filters.pageSize))
  if (filters.name) params.set('name', filters.name)
  const qs = params.toString()
  return apiFetch<PaginatedResponse<Artist>>(`${MUSIC_API_URL}/api/v1/artists${qs ? `?${qs}` : ''}`)
}

export async function getArtist(id: number): Promise<Artist> {
  return apiFetch<Artist>(`${MUSIC_API_URL}/api/v1/artists/${id}`)
}

export async function createArtist(data: ArtistCreatePayload): Promise<Artist> {
  return apiFetch<Artist>(`${MUSIC_API_URL}/api/v1/artists`, { method: 'POST', body: JSON.stringify(data) })
}

export async function updateArtist(id: number, data: Partial<ArtistCreatePayload>): Promise<Artist> {
  return apiFetch<Artist>(`${MUSIC_API_URL}/api/v1/artists/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}

export async function deleteArtist(id: number): Promise<void> {
  return apiFetch<void>(`${MUSIC_API_URL}/api/v1/artists/${id}`, { method: 'DELETE' })
}

export async function getAlbums(filters: MusicFilters = {}): Promise<PaginatedResponse<Album>> {
  const params = new URLSearchParams()
  if (filters.page) params.set('page', String(filters.page))
  if (filters.pageSize) params.set('pageSize', String(filters.pageSize))
  if (filters.title) params.set('title', filters.title)
  const qs = params.toString()
  return apiFetch<PaginatedResponse<Album>>(`${MUSIC_API_URL}/api/v1/albums${qs ? `?${qs}` : ''}`)
}

export async function getAlbum(id: number): Promise<Album> {
  return apiFetch<Album>(`${MUSIC_API_URL}/api/v1/albums/${id}`)
}

export async function createAlbum(data: AlbumCreatePayload): Promise<Album> {
  return apiFetch<Album>(`${MUSIC_API_URL}/api/v1/albums`, { method: 'POST', body: JSON.stringify(data) })
}

export async function updateAlbum(id: number, data: Partial<AlbumCreatePayload>): Promise<Album> {
  return apiFetch<Album>(`${MUSIC_API_URL}/api/v1/albums/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}

export async function deleteAlbum(id: number): Promise<void> {
  return apiFetch<void>(`${MUSIC_API_URL}/api/v1/albums/${id}`, { method: 'DELETE' })
}

export async function getTracks(filters: MusicFilters = {}): Promise<PaginatedResponse<Track>> {
  const params = new URLSearchParams()
  if (filters.page) params.set('page', String(filters.page))
  if (filters.pageSize) params.set('pageSize', String(filters.pageSize))
  if (filters.title) params.set('title', filters.title)
  const qs = params.toString()
  return apiFetch<PaginatedResponse<Track>>(`${MUSIC_API_URL}/api/v1/tracks${qs ? `?${qs}` : ''}`)
}

export async function getTrack(id: number): Promise<Track> {
  return apiFetch<Track>(`${MUSIC_API_URL}/api/v1/tracks/${id}`)
}

export async function createTrack(data: TrackCreatePayload): Promise<Track> {
  return apiFetch<Track>(`${MUSIC_API_URL}/api/v1/tracks`, { method: 'POST', body: JSON.stringify(data) })
}

export async function updateTrack(id: number, data: Partial<TrackCreatePayload>): Promise<Track> {
  return apiFetch<Track>(`${MUSIC_API_URL}/api/v1/tracks/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}

export async function deleteTrack(id: number): Promise<void> {
  return apiFetch<void>(`${MUSIC_API_URL}/api/v1/tracks/${id}`, { method: 'DELETE' })
}

// --- Genre API ---
export async function getGenres(filters: GenreFilters = {}): Promise<PaginatedResponse<Genre>> {
  const params = new URLSearchParams()
  if (filters.page) params.set('page', String(filters.page))
  if (filters.pageSize) params.set('pageSize', String(filters.pageSize))
  if (filters.name) params.set('name', filters.name)
  const qs = params.toString()
  return apiFetch<PaginatedResponse<Genre>>(`${MUSIC_API_URL}/api/v1/genres${qs ? `?${qs}` : ''}`)
}

export async function getGenre(id: number): Promise<Genre> {
  return apiFetch<Genre>(`${MUSIC_API_URL}/api/v1/genres/${id}`)
}

export async function createGenre(data: GenreCreatePayload): Promise<Genre> {
  return apiFetch<Genre>(`${MUSIC_API_URL}/api/v1/genres`, { method: 'POST', body: JSON.stringify(data) })
}

export async function updateGenre(id: number, data: Partial<GenreCreatePayload>): Promise<Genre> {
  return apiFetch<Genre>(`${MUSIC_API_URL}/api/v1/genres/${id}`, { method: 'PUT', body: JSON.stringify(data) })
}

export async function deleteGenre(id: number): Promise<void> {
  return apiFetch<void>(`${MUSIC_API_URL}/api/v1/genres/${id}`, { method: 'DELETE' })
}