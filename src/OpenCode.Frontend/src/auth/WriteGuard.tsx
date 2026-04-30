import { type ReactNode } from 'react'
import { useAuth } from './AuthProvider'

interface WriteGuardProps {
  children: ReactNode
  role?: string
  fallback?: ReactNode
}

export default function WriteGuard({ children, role = 'editor', fallback = null }: WriteGuardProps) {
  const { isAuthenticated, isLoading, hasRole } = useAuth()

  if (isLoading) return null
  if (!isAuthenticated) return fallback
  if (!hasRole(role)) return fallback
  return <>{children}</>
}
