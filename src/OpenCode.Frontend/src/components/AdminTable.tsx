import { useState, useMemo, type ReactNode } from 'react'
import Pagination from './Pagination'

interface AdminColumn<T> {
  key: string
  label: string
  sortable?: boolean
  render?: (item: T) => ReactNode
}

interface AdminTableProps<T> {
  columns: AdminColumn<T>[]
  data: T[]
  keyExtractor: (item: T) => string | number
  onEdit?: (item: T) => void
  onDelete?: (item: T) => void
  isLoading?: boolean
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
  onPageChange: (page: number) => void
}

export default function AdminTable<T extends Record<string, unknown>>({
  columns, data, keyExtractor, onEdit, onDelete,
  isLoading, page, totalPages, onPageChange,
}: AdminTableProps<T>) {
  const [search, setSearch] = useState('')
  const [sortKey, setSortKey] = useState<string | null>(null)
  const [sortDir, setSortDir] = useState<'asc' | 'desc'>('asc')

  const filtered = useMemo(() => {
    if (!search) return data
    const q = search.toLowerCase()
    return data.filter(item =>
      columns.some(col => {
        const val = item[col.key]
        return val != null && String(val).toLowerCase().includes(q)
      })
    )
  }, [data, search, columns])

  const sorted = useMemo(() => {
    if (!sortKey) return filtered
    return [...filtered].sort((a, b) => {
      const aVal = a[sortKey]; const bVal = b[sortKey]
      if (aVal == null) return 1; if (bVal == null) return -1
      const cmp = String(aVal).localeCompare(String(bVal))
      return sortDir === 'asc' ? cmp : -cmp
    })
  }, [filtered, sortKey, sortDir])

  const handleSort = (key: string) => {
    if (sortKey === key) setSortDir(d => d === 'asc' ? 'desc' : 'asc')
    else { setSortKey(key); setSortDir('asc') }
  }

  if (isLoading) return <div className="p-8 text-center">Loading...</div>

  return (
    <div>
      <div className="mb-4">
        <input
          type="text" placeholder="Search..."
          value={search} onChange={e => setSearch(e.target.value)}
          className="w-full p-2 border rounded"
        />
      </div>
      <table className="w-full border-collapse">
        <thead>
          <tr className="bg-gray-100 text-left">
            {columns.map(col => (
              <th key={col.key}
                className={`p-3 border-b-2 border-gray-300 ${col.sortable ? 'cursor-pointer hover:bg-gray-200 select-none' : ''}`}
                onClick={() => col.sortable && handleSort(col.key)}>
                {col.label}
                {sortKey === col.key && (sortDir === 'asc' ? ' ▲' : ' ▼')}
              </th>
            ))}
            {(onEdit || onDelete) && <th className="p-3 border-b-2 border-gray-300">Actions</th>}
          </tr>
        </thead>
        <tbody>
          {sorted.map(item => (
            <tr key={keyExtractor(item)} className="border-b border-gray-200 hover:bg-gray-50">
              {columns.map(col => (
                <td key={col.key} className="p-2.5 px-3">
                  {col.render ? col.render(item) : String(item[col.key] ?? '')}
                </td>
              ))}
              {(onEdit || onDelete) && (
                <td className="p-2.5 px-3">
                  <div className="flex gap-2">
                    {onEdit && (
                      <button onClick={() => onEdit(item)} className="px-3 py-1 bg-blue-500 text-white rounded text-sm hover:bg-blue-600">
                        Edit
                      </button>
                    )}
                    {onDelete && (
                      <button onClick={() => onDelete(item)} className="px-3 py-1 bg-red-500 text-white rounded text-sm hover:bg-red-600">
                        Delete
                      </button>
                    )}
                  </div>
                </td>
              )}
            </tr>
          ))}
        </tbody>
      </table>
      <div className="mt-4">
        <Pagination page={page} totalPages={totalPages} onPageChange={onPageChange} />
      </div>
    </div>
  )
}
