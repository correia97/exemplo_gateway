export interface PaginatedResponse<T> {
  data: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiError {
  message: string;
  correlationId?: string;
  statusCode: number;
}

// Dragon Ball domain types
export interface Character {
  id: number;
  name: string;
  race: string;
  ki: string;
  maxKi: string;
  description?: string;
  imageUrl?: string;
  transformations: Transformation[];
  planet: Planet | null;
  createdAt: string;
  updatedAt: string;
}

export interface Transformation {
  id: number;
  name: string;
  ki: string;
  description?: string;
  imageUrl?: string;
}

export interface Planet {
  id: number;
  name: string;
}

export interface CharacterFilters {
  name?: string;
  race?: string;
  minKi?: number;
  maxKi?: number;
  page?: number;
  pageSize?: number;
}

export interface CharacterCreatePayload {
  name: string;
  race: string;
  ki: string;
  maxKi?: string;
  description?: string;
  imageUrl?: string;
  planetId?: number;
}

// Music domain types
export interface Artist {
  id: number;
  name: string;
  genre?: string;
  biography?: string;
  imageUrl?: string;
  albums: Album[];
  createdAt: string;
  updatedAt: string;
}

export interface Album {
  id: number;
  title: string;
  releaseYear?: number;
  genre?: string;
  coverUrl?: string;
  artistId: number;
  artistName?: string;
  tracks: Track[];
  createdAt: string;
  updatedAt: string;
}

export interface Track {
  id: number;
  title: string;
  duration?: number;
  trackNumber?: number;
  lyrics?: string;
  albumId: number;
  albumTitle?: string;
  createdAt: string;
  updatedAt: string;
}

export interface ArtistCreatePayload {
  name: string;
  genre?: string;
  biography?: string;
}

export interface AlbumCreatePayload {
  title: string;
  releaseYear?: number;
  genre?: string;
  artistId: number;
}

export interface TrackCreatePayload {
  title: string;
  duration?: number;
  trackNumber?: number;
  lyrics?: string;
  albumId: number;
}

export interface MusicFilters {
  name?: string;
  title?: string;
  page?: number;
  pageSize?: number;
}
