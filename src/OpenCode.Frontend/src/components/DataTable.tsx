import type { ReactNode } from 'react'

interface Column<T> {
  key: string
  label: string
  render?: (item: T) => ReactNode
}

interface DataTableProps<T> {
  columns: Column<T>[]
  data: T[]
  keyExtractor: (item: T) => string | number
  onRowClick?: (item: T) => void
  isLoading?: boolean
}

export default function DataTable<T extends object>({
  columns, data, keyExtractor, onRowClick, isLoading,
}: DataTableProps<T>) {
  if (isLoading) return <div className="p-8 text-center">Loading...</div>
  return (
    <table className="w-full border-collapse">
      <thead>
        <tr className="bg-gray-100 text-left">
          {columns.map(col => <th key={col.key} className="p-3 border-b-2 border-gray-300">{col.label}</th>)}
        </tr>
      </thead>
      <tbody>
        {data.map(item => (
          <tr
            key={keyExtractor(item)}
            onClick={() => onRowClick?.(item)}
            className={`border-b border-gray-200 hover:bg-gray-50 ${onRowClick ? 'cursor-pointer' : ''}`}
          >
            {columns.map(col => (
              <td key={col.key} className="p-2.5 px-3">
                  {col.render ? col.render(item) : String((item as Record<string, unknown>)[col.key] ?? '')}
              </td>
            ))}
          </tr>
        ))}
      </tbody>
    </table>
  )
}
