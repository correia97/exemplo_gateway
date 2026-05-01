import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import AdminTable from '../../components/AdminTable'
import ConfirmDialog from '../../components/ConfirmDialog'
import { getAlbums, deleteAlbum } from '../../api/music'
import type { Album } from '../../api/types'

export default function AlbumsPage() {
  const navigate = useNavigate()
  const [albums, setAlbums] = useState<Album[]>([])
  const [page, setPage] = useState(1); const pageSize = 10
  const [totalCount, setTotalCount] = useState(0); const [totalPages, setTotalPages] = useState(1)
  const [isLoading, setIsLoading] = useState(true)
  const [deleteTarget, setDeleteTarget] = useState<Album | null>(null)
  const [isDeleting, setIsDeleting] = useState(false)
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null)

  const load = useCallback(async () => {
    setIsLoading(true)
    try {
      const result = await getAlbums({ page, pageSize })
      setAlbums(result.data)
      setTotalCount(result.totalCount)
      setTotalPages(result.totalPages)
    } catch { setMessage({ type: 'error', text: 'Failed to load albums' })
    } finally { setIsLoading(false) }
  }, [page, pageSize])

  useEffect(() => { load() }, [load])

  const handleDelete = async () => {
    if (!deleteTarget) return
    setIsDeleting(true)
    try {
      await deleteAlbum(deleteTarget.id)
      setMessage({ type: 'success', text: 'Album deleted successfully' })
      setDeleteTarget(null); load()
    } catch { setMessage({ type: 'error', text: 'Failed to delete album' })
    } finally { setIsDeleting(false) }
  }

  const columns = [
    { key: 'title', label: 'Title', sortable: true },
    { key: 'artistName', label: 'Artist', sortable: true },
    { key: 'releaseYear', label: 'Release Year', sortable: true },
    { key: 'genre', label: 'Genre' },
    { key: 'createdAt', label: 'Created', sortable: true, render: (item: any) => new Date(item.createdAt).toLocaleDateString() },
    { key: 'updatedAt', label: 'Updated', sortable: true, render: (item: any) => new Date(item.updatedAt).toLocaleDateString() },
  ]

  return (
    <div>
      <h1 className="text-2xl font-bold mb-4">Albums ({totalCount})</h1>
      {message && (
        <div className={`mb-4 px-4 py-2 rounded ${message.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
          {message.text}
          <button onClick={() => setMessage(null)} className="float-right font-bold">&times;</button>
        </div>
      )}
      <AdminTable<Album>
        columns={columns} data={albums} keyExtractor={a => a.id}
        onEdit={a => navigate(`/music/artists/${a.artistId}`)}
        onDelete={a => setDeleteTarget(a)}
        isLoading={isLoading} page={page} pageSize={pageSize}
        totalCount={totalCount} totalPages={totalPages}
        onPageChange={setPage}
      />
      <ConfirmDialog
        isOpen={!!deleteTarget} title="Delete Album"
        message="Are you sure you want to delete this album?"
        entityName={deleteTarget?.title ?? ''} entityType="Album"
        onConfirm={handleDelete} onCancel={() => setDeleteTarget(null)}
        isLoading={isDeleting}
      />
    </div>
  )
}
