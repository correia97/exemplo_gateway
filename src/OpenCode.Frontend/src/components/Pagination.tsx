interface PaginationProps {
  page: number
  totalPages: number
  onPageChange: (page: number) => void
}

export default function Pagination({ page, totalPages, onPageChange }: PaginationProps) {
  if (totalPages <= 1) return null
  const btnClass = 'px-3 py-1.5 border border-gray-300 rounded bg-white cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed hover:bg-gray-50'
  return (
    <div className="flex gap-2 justify-center p-4 items-center">
      <button disabled={page <= 1} onClick={() => onPageChange(page - 1)} className={btnClass}>Previous</button>
      <span className="text-sm">Page {page} of {totalPages}</span>
      <button disabled={page >= totalPages} onClick={() => onPageChange(page + 1)} className={btnClass}>Next</button>
    </div>
  )
}
