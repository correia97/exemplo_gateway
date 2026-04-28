import { useState, useEffect, useRef } from 'react'
import DataTable from '../../components/DataTable'
import Pagination from '../../components/Pagination'
import EmptyState from '../../components/EmptyState'
import WriteGuard from '../../auth/WriteGuard'
import { getCharacters } from '../../api/dragonball'
import type { Character } from '../../api/types'

interface Props {
  onSelect: (id: number) => void
  onCreate: () => void
}

export default function CharacterList({ onSelect, onCreate }: Props) {
  const [characters, setCharacters] = useState<Character[]>([])
  const [page, setPage] = useState(1)
  const [totalPages, setTotalPages] = useState(1)
  const [totalCount, setTotalCount] = useState(0)
  const [isLoading, setIsLoading] = useState(true)
  const [search, setSearch] = useState('')
  const [raceFilter, setRaceFilter] = useState('')
  const pageSize = 10
  const fetchId = useRef(0)

  useEffect(() => {
    const id = ++fetchId.current
    setIsLoading(id === 1)

    const filters: Record<string, unknown> = { page, pageSize }
    if (search) filters.name = search
    if (raceFilter) filters.race = raceFilter

    getCharacters(filters).then(result => {
      if (id !== fetchId.current) return
      setCharacters(result.data)
      setTotalPages(result.totalPages)
      setTotalCount(result.totalCount)
      setIsLoading(false)
    }).catch(() => {
      if (id !== fetchId.current) return
      setCharacters([])
      setIsLoading(false)
    })
  }, [page, search, raceFilter])

  const handleSearch = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearch(e.target.value)
    setPage(1)
  }
  const handleRaceFilter = (e: React.ChangeEvent<HTMLSelectElement>) => {
    setRaceFilter(e.target.value)
    setPage(1)
  }

  const columns = [
    { key: 'name', label: 'Name' },
    { key: 'race', label: 'Race' },
    { key: 'ki', label: 'Ki' },
    { key: 'transformations', label: 'Transformations', render: (item: Character) => String(item.transformations?.length ?? 0) },
  ]

  return (
    <div>
      <div className="flex gap-4 mb-4 items-center flex-wrap">
        <input
          type="text"
          placeholder="Search by name..."
          value={search}
          onChange={handleSearch}
          className="px-3 py-1.5 border border-gray-300 rounded flex-1 min-w-[200px]"
        />
        <select value={raceFilter} onChange={handleRaceFilter} className="px-3 py-1.5 border border-gray-300 rounded">
          <option value="">All Races</option>
          <option value="Saiyan">Saiyan</option>
          <option value="Human">Human</option>
          <option value="Namekian">Namekian</option>
          <option value="Frieza Race">Frieza Race</option>
          <option value="Android">Android</option>
          <option value="Majin">Majin</option>
          <option value="Angel">Angel</option>
          <option value="God">God</option>
          <option value="Other">Other</option>
        </select>
        <span className="text-sm text-gray-500">{totalCount} characters</span>
        <WriteGuard>
          <button onClick={onCreate} className="px-4 py-1.5 bg-blue-600 text-white rounded cursor-pointer ml-auto hover:bg-blue-700">
            + New Character
          </button>
        </WriteGuard>
      </div>
      {characters.length === 0 && !isLoading ? (
        <WriteGuard fallback={<EmptyState message="No characters found" />}>
          <EmptyState message="No characters found" actionLabel="Create one" onAction={onCreate} />
        </WriteGuard>
      ) : (
        <>
          <DataTable<Character>
            columns={columns}
            data={characters}
            keyExtractor={c => c.id}
            onRowClick={c => onSelect(c.id)}
            isLoading={isLoading}
          />
          <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
        </>
      )}
    </div>
  )
}
