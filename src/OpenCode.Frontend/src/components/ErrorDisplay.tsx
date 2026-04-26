interface ErrorDisplayProps {
  message: string
  correlationId?: string
  onRetry?: () => void
  onDismiss?: () => void
}

export default function ErrorDisplay({ message, correlationId, onRetry, onDismiss }: ErrorDisplayProps) {
  return (
    <div className="rounded-lg border border-red-200 bg-red-50 p-4 my-4">
      <div className="flex items-start gap-3">
        <span className="text-red-500 text-lg mt-0.5 shrink-0" role="img" aria-label="error">&#x26A0;</span>
        <div className="flex-1 min-w-0">
          <p className="text-sm text-red-800">{message}</p>
          {correlationId && (
            <p className="text-xs text-red-400 mt-1 font-mono">ID: {correlationId}</p>
          )}
        </div>
        <div className="flex gap-2 shrink-0">
          {onRetry && (
            <button
              onClick={onRetry}
              className="text-sm px-3 py-1 rounded bg-red-600 text-white cursor-pointer hover:bg-red-700"
            >
              Retry
            </button>
          )}
          {onDismiss && (
            <button
              onClick={onDismiss}
              className="text-sm px-2 py-1 rounded text-red-600 hover:bg-red-100 cursor-pointer"
            >
              &times;
            </button>
          )}
        </div>
      </div>
    </div>
  )
}
