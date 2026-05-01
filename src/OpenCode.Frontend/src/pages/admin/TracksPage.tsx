import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import AdminTable from '../../components/AdminTable'
import ConfirmDialog from '../../components/ConfirmDialog'
import { getTracks, deleteTrack } from '../../api/music'
import type { Track } from '../../api/types'

export default function TracksPage() {
  const navigate = useNavigate()
  const [tracks, setTracks] = useState<Track[]>([])
  const [page, setPage] = useState(1); const pageSize = 10
  const [totalCount, setTotalCount] = useState(0); const [totalPages, setTotalPages] = useState(1)
  const [isLoading, setIsLoading] = useState(true)
  const [deleteTarget, setDeleteTarget] = useState<Track | null>(null)
  const [isDeleting, setIsDeleting] = useState(false)
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null)

  const load = useCallback(async () => {
    setIsLoading(true)
    try {
      const result = await getTracks({ page, pageSize })
      setTracks(result.data)
      setTotalCount(result.totalCount)
      setTotalPages(result.totalPages)
    } catch { setMessage({ type: 'error', text: 'Failed to load tracks' })
    } finally { setIsLoading(false) }
  }, [page, pageSize])

  useEffect(() => { load() }, [load])

  const handleDelete = async () => {
    if (!deleteTarget) return
    setIsDeleting(true)
    try {
      await deleteTrack(deleteTarget.id)
      setMessage({ type: 'success', text: 'Track deleted successfully' })
      setDeleteTarget(null); load()
    } catch { setMessage({ type: 'error', text: 'Failed to delete track' })
    } finally { setIsDeleting(false) }
  }

  const columns = [
    { key: 'title', label: 'Title', sortable: true },
    { key: 'trackNumber', label: 'Track #', sortable: true, render: (item: any) => item.trackNumber ?? '-' },
    { key: 'duration', label: 'Duration', render: (item: any) => item.duration ? `${item.duration}s` : '-' },
    { key: 'albumTitle', label: 'Album', sortable: true },
    { key: 'createdAt', label: 'Created', sortable: true, render: (item: any) => new Date(item.createdAt).toLocaleDateString() },
    { key: 'updatedAt', label: 'Updated', sortable: true, render: (item: any) => new Date(item.updatedAt).toLocaleDateString() },
  ]

  return (
    <div>
      <h1 className="text-2xl font-bold mb-4">Tracks ({totalCount})</h1>
      {message && (
        <div className={`mb-4 px-4 py-2 rounded ${message.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
          {message.text}
          <button onClick={() => setMessage(null)} className="float-right font-bold">&times;</button>
        </div>
      )}
      <AdminTable<Track>
        columns={columns} data={tracks} keyExtractor={t => t.id}
        onEdit={t => navigate(`/music/albums/${t.albumId}`)}
        onDelete={t => setDeleteTarget(t)}
        isLoading={isLoading} page={page} pageSize={pageSize}
        totalCount={totalCount} totalPages={totalPages}
        onPageChange={setPage}
      />
      <ConfirmDialog
        isOpen={!!deleteTarget} title="Delete Track"
        message="Are you sure you want to delete this track?"
        entityName={deleteTarget?.title ?? ''} entityType="Track"
        onConfirm={handleDelete} onCancel={() => setDeleteTarget(null)}
        isLoading={isDeleting}
      />
    </div>
  )
}
