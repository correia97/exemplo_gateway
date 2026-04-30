import { useState, useCallback } from 'react'
import ArtistList from './ArtistList'
import ArtistDetail from './ArtistDetail'
import AlbumDetail from './AlbumDetail'
import MusicForm from './MusicForm'
import { useToast } from '../../hooks/useApiError'
import type { Artist, Album, ArtistCreatePayload, AlbumCreatePayload, TrackCreatePayload } from '../../api/types'
import { getArtist, getAlbum, createArtist, updateArtist, deleteArtist, createAlbum, updateAlbum, deleteAlbum, createTrack, updateTrack } from '../../api/music'

type View = 'artist-list' | 'artist-detail' | 'album-detail' | 'create-artist' | 'edit-artist' | 'create-album' | 'edit-album' | 'create-track' | 'edit-track'

export default function MusicPage() {
  const [view, setView] = useState<View>('artist-list')
  const [selectedArtist, setSelectedArtist] = useState<Artist | null>(null)
  const [selectedAlbum, setSelectedAlbum] = useState<Album | null>(null)
  const { toasts, handleError, dismissToast } = useToast()

  const handleSelectArtist = useCallback(async (id: number) => {
    try {
      const artist = await getArtist(id)
      setSelectedArtist(artist)
      setView('artist-detail')
    } catch (e: unknown) {
      handleError(e)
    }
  }, [handleError])

  const handleSelectAlbum = useCallback(async (id: number) => {
    try {
      const album = await getAlbum(id)
      setSelectedAlbum(album)
      setView('album-detail')
    } catch (e: unknown) {
      handleError(e)
    }
  }, [handleError])

  const handleCreateArtist = useCallback(async (data: ArtistCreatePayload) => {
    try { await createArtist(data); setView('artist-list') } catch (e: unknown) { handleError(e) }
  }, [handleError])
  const handleUpdateArtist = useCallback(async (id: number, data: Partial<ArtistCreatePayload>) => {
    try { setSelectedArtist(await updateArtist(id, data)); setView('artist-detail') } catch (e: unknown) { handleError(e) }
  }, [handleError])
  const handleDeleteArtist = useCallback(async (id: number) => {
    if (!window.confirm('Delete this artist and all their albums?')) return
    try { await deleteArtist(id); setView('artist-list'); setSelectedArtist(null) } catch (e: unknown) { handleError(e) }
  }, [handleError])

  const handleCreateAlbum = useCallback(async (data: AlbumCreatePayload) => {
    try { await createAlbum(data); if (selectedArtist) setSelectedArtist(await getArtist(selectedArtist.id)); setView('artist-detail') } catch (e: unknown) { handleError(e) }
  }, [selectedArtist, handleError])
  const handleUpdateAlbum = useCallback(async (id: number, data: Partial<AlbumCreatePayload>) => {
    try { await updateAlbum(id, data); if (selectedAlbum) setSelectedAlbum(await getAlbum(id)); setView('album-detail') } catch (e: unknown) { handleError(e) }
  }, [selectedAlbum, handleError])
  const handleDeleteAlbum = useCallback(async (id: number) => {
    if (!window.confirm('Delete this album and all its tracks?')) return
    try { await deleteAlbum(id); if (selectedArtist) setSelectedArtist(await getArtist(selectedArtist.id)); setView('artist-detail') } catch (e: unknown) { handleError(e) }
  }, [selectedArtist, handleError])

  const handleCreateTrack = useCallback(async (data: TrackCreatePayload) => {
    try { await createTrack(data); if (selectedAlbum) setSelectedAlbum(await getAlbum(selectedAlbum.id)); setView('album-detail') } catch (e: unknown) { handleError(e) }
  }, [selectedAlbum, handleError])
  const handleUpdateTrack = useCallback(async (id: number, data: Partial<TrackCreatePayload>) => {
    try { await updateTrack(id, data); if (selectedAlbum) setSelectedAlbum(await getAlbum(selectedAlbum.id)); setView('album-detail') } catch (e: unknown) { handleError(e) }
  }, [selectedAlbum, handleError])
  const handleBack = useCallback(() => {
    if (view === 'artist-detail' || view.startsWith('create-') || view.startsWith('edit-')) {
      if (view === 'artist-detail') setView('artist-list')
      else if (view === 'album-detail' || view.startsWith('create-track') || view.startsWith('edit-track')) {
        if (selectedAlbum) setSelectedAlbum(null); setView('artist-detail')
      } else if (view.startsWith('create-album') || view.startsWith('edit-album')) setView('artist-detail')
      else setView('artist-list')
    } else setView('artist-list')
  }, [view, selectedAlbum])

  return (
    <div>
      {toasts.length > 0 && (
        <div className="fixed top-4 right-4 z-50 flex flex-col gap-2">
          {toasts.map(t => (
            <div key={t.id} className="px-4 py-3 rounded shadow-lg text-white bg-red-600 flex items-center gap-3 min-w-[300px]">
              <span className="flex-1 text-sm">{t.message}</span>
              {t.correlationId && <span className="text-xs text-red-200 font-mono">{t.correlationId}</span>}
              <button onClick={() => dismissToast(t.id)} className="text-white/80 hover:text-white text-lg leading-none">&times;</button>
            </div>
          ))}
        </div>
      )}
      <h1 className="text-2xl font-bold mb-4">Music Catalog</h1>

      {view === 'artist-list' && (
        <ArtistList onSelect={handleSelectArtist} onCreate={() => setView('create-artist')} />
      )}
      {view === 'artist-detail' && selectedArtist && (
        <ArtistDetail
          artist={selectedArtist}
          onSelectAlbum={(id) => handleSelectAlbum(id)}
          onCreateAlbum={() => setView('create-album')}
          onEdit={() => setView('edit-artist')}
          onDelete={() => handleDeleteArtist(selectedArtist.id)}
          onBack={handleBack}
        />
      )}
      {view === 'album-detail' && selectedAlbum && (
        <AlbumDetail
          album={selectedAlbum}
          onCreateTrack={() => setView('create-track')}
          onEdit={() => setView('edit-album')}
          onDelete={() => handleDeleteAlbum(selectedAlbum.id)}
          onBack={handleBack}
        />
      )}
      {view === 'create-artist' && (
        <MusicForm mode="create-artist" onSubmit={(d) => handleCreateArtist(d as ArtistCreatePayload)} onCancel={handleBack} />
      )}
      {view === 'edit-artist' && selectedArtist && (
        <MusicForm mode="edit-artist" initial={selectedArtist} onSubmit={(d) => handleUpdateArtist(selectedArtist.id, d as Partial<ArtistCreatePayload>)} onCancel={handleBack} />
      )}
      {view === 'create-album' && selectedArtist && (
        <MusicForm mode="create-album" initial={{ artistId: selectedArtist.id }} onSubmit={(d) => handleCreateAlbum(d as AlbumCreatePayload)} onCancel={handleBack} />
      )}
      {view === 'edit-album' && selectedAlbum && (
        <MusicForm mode="edit-album" initial={selectedAlbum} onSubmit={(d) => handleUpdateAlbum(selectedAlbum.id, d as Partial<AlbumCreatePayload>)} onCancel={handleBack} />
      )}
      {view === 'create-track' && selectedAlbum && (
        <MusicForm mode="create-track" initial={{ albumId: selectedAlbum.id }} onSubmit={(d) => handleCreateTrack(d as TrackCreatePayload)} onCancel={handleBack} />
      )}
    </div>
  )
}
