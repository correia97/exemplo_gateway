interface ConfirmDialogProps {
  isOpen: boolean
  title: string
  message: string
  entityName: string
  entityType: string
  onConfirm: () => void
  onCancel: () => void
  isLoading?: boolean
}

export default function ConfirmDialog({
  isOpen, title, message, entityName, entityType,
  onConfirm, onCancel, isLoading,
}: ConfirmDialogProps) {
  if (!isOpen) return null

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50">
      <div className="bg-white rounded-lg shadow-xl p-6 max-w-md w-full">
        <h3 className="text-lg font-bold text-red-600 mb-2">{title}</h3>
        <p className="text-gray-600 mb-4">{message}</p>
        <div className="bg-gray-50 rounded p-3 mb-4">
          <p><strong>Type:</strong> {entityType}</p>
          <p><strong>Name:</strong> {entityName}</p>
        </div>
        <div className="flex justify-end gap-3">
          <button onClick={onCancel} disabled={isLoading}
            className="px-4 py-2 bg-gray-200 rounded hover:bg-gray-300">
            Cancel
          </button>
          <button onClick={onConfirm} disabled={isLoading}
            className="px-4 py-2 bg-red-600 text-white rounded hover:bg-red-700 disabled:opacity-50">
            {isLoading ? 'Deleting...' : 'Delete'}
          </button>
        </div>
      </div>
    </div>
  )
}
