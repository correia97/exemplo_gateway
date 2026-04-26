interface EmptyStateProps { message: string; actionLabel?: string; onAction?: () => void }
export default function EmptyState({ message, actionLabel, onAction }: EmptyStateProps) {
  return (
    <div className="text-center p-12 text-gray-400">
      <p>{message}</p>
      {actionLabel && onAction && (
        <button onClick={onAction} className="mt-4 px-6 py-2 cursor-pointer border border-gray-300 rounded bg-white hover:bg-gray-50">
          {actionLabel}
        </button>
      )}
    </div>
  )
}
