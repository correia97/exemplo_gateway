import { useState, useEffect, useRef } from 'react'
import DataTable from '../../components/DataTable'
import Pagination from '../../components/Pagination'
import EmptyState from '../../components/EmptyState'
import ErrorDisplay from '../../components/ErrorDisplay'
import WriteGuard from '../../auth/WriteGuard'
import { getArtists } from '../../api/music'
import type { Artist } from '../../api/types'

interface Props { onSelect: (id: number) => void; onCreate: () => void }

export default function ArtistList({ onSelect, onCreate }: Props) {
  const [artists, setArtists] = useState<Artist[]>([])
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)
  const [retryCount, setRetryCount] = useState(0)
  const [search, setSearch] = useState('')
  const pageSize = 10
  const fetchId = useRef(0)

  useEffect(() => {
    const id = ++fetchId.current
    setIsLoading(id === 1)
    setError(null)

    getArtists({ page, pageSize, name: search || undefined }).then(result => {
      if (id !== fetchId.current) return
      setArtists(result.data)
      setTotalPages(result.totalPages)
      setTotalCount(result.totalCount)
      setIsLoading(false)
    }).catch((err: unknown) => {
      if (id !== fetchId.current) return
      setError(err instanceof Error ? err.message : 'Failed to load artists')
      setArtists([])
      setIsLoading(false)
    })
  }, [page, search, retryCount])

  const columns = [
    { key: 'name', label: 'Name' },
    { key: 'genre', label: 'Genre' },
    { key: 'albums', label: 'Albums', render: (a: Artist) => String(a.albums?.length ?? 0) },
  ]

  return (
    <div>
      <div className="flex items-center gap-4 mb-4">
        <input type="text" placeholder="Search artists..." value={search} onChange={e => { setSearch(e.target.value); setPage(1) }}
          className="flex-1 min-w-[200px] px-3 py-2 border border-gray-300 rounded text-sm" />
        <span className="text-xs text-gray-500">{totalCount} artists</span>
        <WriteGuard>
          <button onClick={onCreate} className="ml-auto px-4 py-2 bg-indigo-600 text-white rounded text-sm cursor-pointer border-none">+ New Artist</button>
        </WriteGuard>
      </div>
      {error && (
        <ErrorDisplay message={error} onRetry={() => setRetryCount(c => c + 1)} />
      )}
      {!error && artists.length === 0 && !isLoading ? (
        <WriteGuard fallback={<EmptyState message="No artists found" />}>
          <EmptyState message="No artists found" actionLabel="Create one" onAction={onCreate} />
        </WriteGuard>
      ) : (
        <>
          <DataTable columns={columns} data={artists} keyExtractor={a => a.id} onRowClick={a => onSelect(a.id)} isLoading={isLoading} />
          <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
        </>
      )}
    </div>
  )
}
