import apiFetch, { DRAGONBALL_API_URL, MUSIC_API_URL } from './client'

export interface AdminStats {
  characters: number
  genres: number
  artists: number
  albums: number
  tracks: number
}

/**
 * Fetch Dragon Ball entity count by calling list endpoint with pageSize=1.
 * Reads totalCount from the paginated response. No dedicated stats endpoint.
 */
export async function fetchDragonBallStats(): Promise<{ characters: number }> {
  const res = await fetch(`${DRAGONBALL_API_URL}/api/v1/characters?pageSize=1`)
  if (!res.ok) return { characters: 0 }
  const data = await res.json()
  return { characters: data.totalCount ?? 0 }
}

/**
 * Fetch Music entity counts using the same pageSize=1 trick for each entity type.
 */
export async function fetchMusicStats(): Promise<{ genres: number; artists: number; albums: number; tracks: number }> {
  const [genres, artists, albums, tracks] = await Promise.all([
    apiFetch<any>(`${MUSIC_API_URL}/api/v1/genres?pageSize=1`).then(d => d.totalCount ?? 0).catch(() => 0),
    apiFetch<any>(`${MUSIC_API_URL}/api/v1/artists?pageSize=1`).then(d => d.totalCount ?? 0).catch(() => 0),
    apiFetch<any>(`${MUSIC_API_URL}/api/v1/albums?pageSize=1`).then(d => d.totalCount ?? 0).catch(() => 0),
    apiFetch<any>(`${MUSIC_API_URL}/api/v1/tracks?pageSize=1`).then(d => d.totalCount ?? 0).catch(() => 0),
  ])
  return { genres, artists, albums, tracks }
}
