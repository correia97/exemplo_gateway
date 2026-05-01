import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import AdminTable from '../../components/AdminTable'
import ConfirmDialog from '../../components/ConfirmDialog'
import { getArtists, deleteArtist } from '../../api/music'
import type { Artist } from '../../api/types'

export default function ArtistsPage() {
  const navigate = useNavigate()
  const [artists, setArtists] = useState<Artist[]>([])
  const [page, setPage] = useState(1); const pageSize = 10
  const [totalCount, setTotalCount] = useState(0); const [totalPages, setTotalPages] = useState(1)
  const [isLoading, setIsLoading] = useState(true)
  const [deleteTarget, setDeleteTarget] = useState<Artist | null>(null)
  const [isDeleting, setIsDeleting] = useState(false)
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null)

  const load = useCallback(async () => {
    setIsLoading(true)
    try {
      const result = await getArtists({ page, pageSize })
      setArtists(result.data)
      setTotalCount(result.totalCount)
      setTotalPages(result.totalPages)
    } catch { setMessage({ type: 'error', text: 'Failed to load artists' })
    } finally { setIsLoading(false) }
  }, [page, pageSize])

  useEffect(() => { load() }, [load])

  const handleDelete = async () => {
    if (!deleteTarget) return
    setIsDeleting(true)
    try {
      await deleteArtist(deleteTarget.id)
      setMessage({ type: 'success', text: 'Artist deleted successfully' })
      setDeleteTarget(null); load()
    } catch { setMessage({ type: 'error', text: 'Failed to delete artist' })
    } finally { setIsDeleting(false) }
  }

  const columns = [
    { key: 'name', label: 'Name', sortable: true },
    { key: 'genre', label: 'Genre', sortable: true },
    { key: 'biography', label: 'Biography', render: (item: any) => item.biography ? item.biography.substring(0, 60) + (item.biography.length > 60 ? '...' : '') : '-' },
    { key: 'createdAt', label: 'Created', sortable: true, render: (item: any) => new Date(item.createdAt).toLocaleDateString() },
    { key: 'updatedAt', label: 'Updated', sortable: true, render: (item: any) => new Date(item.updatedAt).toLocaleDateString() },
  ]

  return (
    <div>
      <h1 className="text-2xl font-bold mb-4">Artists ({totalCount})</h1>
      {message && (
        <div className={`mb-4 px-4 py-2 rounded ${message.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
          {message.text}
          <button onClick={() => setMessage(null)} className="float-right font-bold">&times;</button>
        </div>
      )}
      <AdminTable<Artist>
        columns={columns} data={artists} keyExtractor={a => a.id}
        onEdit={a => navigate(`/music/artists?edit=${a.id}`)}
        onDelete={a => setDeleteTarget(a)}
        isLoading={isLoading} page={page} pageSize={pageSize}
        totalCount={totalCount} totalPages={totalPages}
        onPageChange={setPage}
      />
      <ConfirmDialog
        isOpen={!!deleteTarget} title="Delete Artist"
        message="Are you sure you want to delete this artist?"
        entityName={deleteTarget?.name ?? ''} entityType="Artist"
        onConfirm={handleDelete} onCancel={() => setDeleteTarget(null)}
        isLoading={isDeleting}
      />
    </div>
  )
}
