import { useState } from 'react'
import type { Artist, Album, Track, ArtistCreatePayload, AlbumCreatePayload, TrackCreatePayload } from '../../api/types'

type FormMode = 'create-artist' | 'edit-artist' | 'create-album' | 'edit-album' | 'create-track' | 'edit-track'

interface Props {
  mode: FormMode
  onSubmit: (data: ArtistCreatePayload | AlbumCreatePayload | TrackCreatePayload) => Promise<void>
  onCancel: () => void
  initial?: Partial<Artist | Album | Track> & { artistId?: number; albumId?: number }
}

export default function MusicForm({ mode, onSubmit, onCancel, initial }: Props) {
  const [name, setName] = useState('')
  const [title, setTitle] = useState('')
  const [genre, setGenre] = useState('')
  const [biography, setBiography] = useState('')
  const [releaseYear, setReleaseYear] = useState('')
  const [duration, setDuration] = useState('')
  const [trackNumber, setTrackNumber] = useState('')
  const [lyrics, setLyrics] = useState('')
  const [submitting, setSubmitting] = useState(false)

  if (initial) {
    if ('name' in initial && name === '') setName((initial as Artist).name || '')
    if ('title' in initial && title === '') setTitle((initial as Album | Track).title || '')
    if ('genre' in initial && genre === '') setGenre((initial as Artist | Album).genre || '')
    if ('biography' in initial && biography === '') setBiography((initial as Artist).biography || '')
    if ('releaseYear' in initial && releaseYear === '') setReleaseYear(String((initial as Album).releaseYear ?? ''))
    if ('duration' in initial && duration === '') setDuration(String((initial as Track).duration ?? ''))
    if ('trackNumber' in initial && trackNumber === '') setTrackNumber(String((initial as Track).trackNumber ?? ''))
    if ('lyrics' in initial && lyrics === '') setLyrics((initial as Track).lyrics || '')
  }

  const isArtistForm = mode === 'create-artist' || mode === 'edit-artist'
  const isAlbumForm = mode === 'create-album' || mode === 'edit-album'
  const isTrackForm = mode === 'create-track' || mode === 'edit-track'
  const isCreate = mode.startsWith('create-')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setSubmitting(true)
    try {
      if (isArtistForm) {
        await onSubmit({ name: name.trim(), genre: genre.trim() || undefined, biography: biography.trim() || undefined } as ArtistCreatePayload)
      } else if (isAlbumForm) {
        await onSubmit({
          title: title.trim(), genre: genre.trim() || undefined,
          releaseYear: releaseYear ? Number(releaseYear) : undefined,
          artistId: (initial as { artistId?: number }).artistId ?? 0,
        } as AlbumCreatePayload)
      } else if (isTrackForm) {
        await onSubmit({
          title: title.trim(), lyrics: lyrics.trim() || undefined,
          duration: duration ? Number(duration) : undefined,
          trackNumber: trackNumber ? Number(trackNumber) : undefined,
          albumId: (initial as { albumId?: number }).albumId ?? 0,
        } as TrackCreatePayload)
      }
    } finally { setSubmitting(false) }
  }

  const inputClass = "w-full px-3 py-2 border border-gray-300 rounded text-sm"
  const labelClass = "block font-semibold mt-3 mb-1 text-sm"

  const titleText = isArtistForm ? 'Artist' : isAlbumForm ? 'Album' : 'Track'
  return (
    <form onSubmit={handleSubmit} className="max-w-[500px]">
      <h2 className="text-xl font-bold mb-4">{isCreate ? `New ${titleText}` : `Edit ${titleText}`}</h2>

      {isArtistForm && (
        <>
          <label className={labelClass}>Name *</label><input className={inputClass} value={name} onChange={e => setName(e.target.value)} required />
          <label className={labelClass}>Genre</label><input className={inputClass} value={genre} onChange={e => setGenre(e.target.value)} />
          <label className={labelClass}>Biography</label><textarea className={`${inputClass} min-h-[80px]`} value={biography} onChange={e => setBiography(e.target.value)} />
        </>
      )}
      {isAlbumForm && (
        <>
          <label className={labelClass}>Title *</label><input className={inputClass} value={title} onChange={e => setTitle(e.target.value)} required />
          <label className={labelClass}>Genre</label><input className={inputClass} value={genre} onChange={e => setGenre(e.target.value)} />
          <label className={labelClass}>Release Year</label><input className={inputClass} type="number" value={releaseYear} onChange={e => setReleaseYear(e.target.value)} />
        </>
      )}
      {isTrackForm && (
        <>
          <label className={labelClass}>Title *</label><input className={inputClass} value={title} onChange={e => setTitle(e.target.value)} required />
          <label className={labelClass}>Duration (seconds)</label><input className={inputClass} type="number" value={duration} onChange={e => setDuration(e.target.value)} />
          <label className={labelClass}>Track Number</label><input className={inputClass} type="number" value={trackNumber} onChange={e => setTrackNumber(e.target.value)} />
          <label className={labelClass}>Lyrics</label><textarea className={`${inputClass} min-h-[100px]`} value={lyrics} onChange={e => setLyrics(e.target.value)} />
        </>
      )}

      <div className="flex gap-2 mt-6">
        <button type="submit" disabled={submitting}
          className={`px-6 py-2 rounded text-white border-none cursor-pointer ${submitting ? 'bg-indigo-400 cursor-not-allowed' : 'bg-indigo-600'}`}>
          {submitting ? 'Saving...' : (isCreate ? 'Create' : 'Save Changes')}
        </button>
        <button type="button" onClick={onCancel} className="px-6 py-2 border border-gray-300 rounded cursor-pointer bg-white text-sm">Cancel</button>
      </div>
    </form>
  )
}
