import { useState, useEffect, useCallback } from 'react'
import AdminTable from '../../components/AdminTable'
import ConfirmDialog from '../../components/ConfirmDialog'
import { getGenres, deleteGenre } from '../../api/music'
import type { Genre } from '../../api/types'

export default function GenresPage() {
  const [genres, setGenres] = useState<Genre[]>([])
  const [page, setPage] = useState(1); const pageSize = 10
  const [totalCount, setTotalCount] = useState(0); const [totalPages, setTotalPages] = useState(1)
  const [isLoading, setIsLoading] = useState(true)
  const [deleteTarget, setDeleteTarget] = useState<Genre | null>(null)
  const [isDeleting, setIsDeleting] = useState(false)
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null)

  const load = useCallback(async () => {
    setIsLoading(true)
    try {
      const result = await getGenres({ page, pageSize })
      setGenres(result.data)
      setTotalCount(result.totalCount)
      setTotalPages(result.totalPages)
    } catch { setMessage({ type: 'error', text: 'Failed to load genres' })
    } finally { setIsLoading(false) }
  }, [page, pageSize])

  useEffect(() => { load() }, [load])

  const handleDelete = async () => {
    if (!deleteTarget) return
    setIsDeleting(true)
    try {
      await deleteGenre(deleteTarget.id)
      setMessage({ type: 'success', text: 'Genre deleted successfully' })
      setDeleteTarget(null); load()
    } catch { setMessage({ type: 'error', text: 'Failed to delete genre' })
    } finally { setIsDeleting(false) }
  }

  const columns = [
    { key: 'name', label: 'Name', sortable: true },
    { key: 'description', label: 'Description' },
    { key: 'createdAt', label: 'Created', sortable: true, render: (item: any) => new Date(item.createdAt).toLocaleDateString() },
    { key: 'updatedAt', label: 'Updated', sortable: true, render: (item: any) => new Date(item.updatedAt).toLocaleDateString() },
  ]

  return (
    <div>
      <h1 className="text-2xl font-bold mb-4">Genres ({totalCount})</h1>
      {message && (
        <div className={`mb-4 px-4 py-2 rounded ${message.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
          {message.text}
          <button onClick={() => setMessage(null)} className="float-right font-bold">&times;</button>
        </div>
      )}
      <AdminTable<Genre>
        columns={columns} data={genres} keyExtractor={g => g.id}
        onDelete={g => setDeleteTarget(g)}
        isLoading={isLoading} page={page} pageSize={pageSize}
        totalCount={totalCount} totalPages={totalPages}
        onPageChange={setPage}
      />
      <ConfirmDialog
        isOpen={!!deleteTarget} title="Delete Genre"
        message="Are you sure you want to delete this genre?"
        entityName={deleteTarget?.name ?? ''} entityType="Genre"
        onConfirm={handleDelete} onCancel={() => setDeleteTarget(null)}
        isLoading={isDeleting}
      />
    </div>
  )
}
