import { useCallback, useState } from 'react'

interface Toast {
  id: number
  message: string
  type: 'error' | 'success' | 'info'
  correlationId?: string
}

let nextId = 0

function extractError(err: unknown): { message: string; correlationId?: string } {
  if (err && typeof err === 'object' && 'response' in err) {
    const axiosErr = err as { response?: { data?: Record<string, unknown> }; message?: string }
    const data = axiosErr.response?.data
    if (data && typeof data === 'object') {
      const detail = typeof data['detail'] === 'string' ? data['detail'] : undefined
      const title = typeof data['title'] === 'string' ? data['title'] : undefined
      const cid = typeof data['correlationId'] === 'string' ? data['correlationId'] : undefined
      return { message: detail ?? title ?? axiosErr.message ?? 'Request failed', correlationId: cid }
    }
    return { message: axiosErr.message ?? 'Request failed' }
  }
  if (err instanceof Error) return { message: err.message }
  if (typeof err === 'string') return { message: err }
  return { message: 'An unexpected error occurred' }
}

export function useApiError() {
  const [toasts, setToasts] = useState<Toast[]>([])

  const showError = useCallback((message: string, correlationId?: string) => {
    const id = nextId++
    setToasts(prev => [...prev, { id, message, correlationId, type: 'error' }])
    setTimeout(() => {
      setToasts(prev => prev.filter(t => t.id !== id))
    }, 5000)
  }, [])

  const handleError = useCallback((err: unknown) => {
    const { message, correlationId } = extractError(err)
    showError(message, correlationId)
  }, [showError])

  const dismissToast = useCallback((id: number) => {
    setToasts(prev => prev.filter(t => t.id !== id))
  }, [])

  return { toasts, showError, handleError, dismissToast }
}

export function useToast() {
  const [toasts, setToasts] = useState<Toast[]>([])

  const showError = useCallback((err: unknown) => {
    const { message, correlationId } = extractError(err)
    const id = nextId++
    setToasts(prev => [...prev, { id, message, correlationId, type: 'error' }])
    setTimeout(() => {
      setToasts(prev => prev.filter(t => t.id !== id))
    }, 5000)
  }, [])

  const handleError = useCallback((err: unknown) => {
    showError(err)
  }, [showError])

  const success = useCallback((msg: string) => {
    const id = nextId++
    setToasts(prev => [...prev, { id, message: msg, type: 'success' }])
    setTimeout(() => {
      setToasts(prev => prev.filter(t => t.id !== id))
    }, 3000)
  }, [])

  const dismissToast = useCallback((id: number) => {
    setToasts(prev => prev.filter(t => t.id !== id))
  }, [])

  return { toasts, showError, handleError, success, dismissToast }
}
