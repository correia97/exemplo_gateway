import { type ReactNode } from 'react'
import { useAuth } from './AuthProvider'

interface AuthGuardProps {
  children: ReactNode
  role?: string
  fallback?: ReactNode
}

export default function AuthGuard({ children, role, fallback = null }: AuthGuardProps) {
  const { isAuthenticated, isLoading, hasRole } = useAuth()

  if (isLoading) return null
  if (!isAuthenticated) return fallback
  if (role && !hasRole(role)) return fallback
  return <>{children}</>
}
