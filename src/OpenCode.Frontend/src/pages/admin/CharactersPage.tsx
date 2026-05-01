import { useState, useEffect, useCallback } from 'react'
import { useNavigate } from 'react-router-dom'
import AdminTable from '../../components/AdminTable'
import ConfirmDialog from '../../components/ConfirmDialog'
import { getCharacters, deleteCharacter } from '../../api/dragonball'
import type { Character } from '../../api/types'

export default function CharactersPage() {
  const navigate = useNavigate()
  const [characters, setCharacters] = useState<Character[]>([])
  const [page, setPage] = useState(1); const pageSize = 10
  const [totalCount, setTotalCount] = useState(0); const [totalPages, setTotalPages] = useState(1)
  const [isLoading, setIsLoading] = useState(true)
  const [deleteTarget, setDeleteTarget] = useState<Character | null>(null)
  const [isDeleting, setIsDeleting] = useState(false)
  const [message, setMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null)

  const load = useCallback(async () => {
    setIsLoading(true)
    try {
      const result = await getCharacters({ page, pageSize })
      setCharacters(result.data)
      setTotalCount(result.totalCount)
      setTotalPages(result.totalPages)
    } catch { setMessage({ type: 'error', text: 'Failed to load characters' })
    } finally { setIsLoading(false) }
  }, [page, pageSize])

  useEffect(() => { load() }, [load])

  const handleDelete = async () => {
    if (!deleteTarget) return
    setIsDeleting(true)
    try {
      await deleteCharacter(deleteTarget.id)
      setMessage({ type: 'success', text: 'Character deleted successfully' })
      setDeleteTarget(null); load()
    } catch { setMessage({ type: 'error', text: 'Failed to delete character' })
    } finally { setIsDeleting(false) }
  }

  const columns = [
    { key: 'name', label: 'Name', sortable: true },
    { key: 'race', label: 'Race', sortable: true },
    { key: 'ki', label: 'Ki', sortable: true },
    { key: 'transformations', label: 'Transformations', render: (item: any) => String(item.transformations?.length ?? 0) },
    { key: 'planet', label: 'Planet', render: (item: any) => item.planet?.name ?? '-' },
    { key: 'createdAt', label: 'Created', sortable: true, render: (item: any) => new Date(item.createdAt).toLocaleDateString() },
    { key: 'updatedAt', label: 'Updated', sortable: true, render: (item: any) => new Date(item.updatedAt).toLocaleDateString() },
  ]

  return (
    <div>
      <h1 className="text-2xl font-bold mb-4">Characters ({totalCount})</h1>
      {message && (
        <div className={`mb-4 px-4 py-2 rounded ${message.type === 'success' ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
          {message.text}
          <button onClick={() => setMessage(null)} className="float-right font-bold">&times;</button>
        </div>
      )}
      <AdminTable<Character>
        columns={columns} data={characters} keyExtractor={c => c.id}
        onEdit={c => navigate(`/dragonball?edit=${c.id}`)}
        onDelete={c => setDeleteTarget(c)}
        isLoading={isLoading} page={page} pageSize={pageSize}
        totalCount={totalCount} totalPages={totalPages}
        onPageChange={setPage}
      />
      <ConfirmDialog
        isOpen={!!deleteTarget} title="Delete Character"
        message="Are you sure you want to delete this character?"
        entityName={deleteTarget?.name ?? ''} entityType="Character"
        onConfirm={handleDelete} onCancel={() => setDeleteTarget(null)}
        isLoading={isDeleting}
      />
    </div>
  )
}
