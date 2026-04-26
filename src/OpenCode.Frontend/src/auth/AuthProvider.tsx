import { createContext, useContext, useState, useEffect, useMemo, type ReactNode } from 'react'
import { userManager, extractRoles } from './config'
import type { User } from 'oidc-client-ts'

interface AuthContextValue {
  user: User | null
  isLoading: boolean
  isAuthenticated: boolean
  login: () => Promise<void>
  logout: () => Promise<void>
  getToken: () => Promise<string | null>
  userRoles: string[]
  hasRole: (role: string) => boolean
}

const AuthContext = createContext<AuthContextValue | null>(null)

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<User | null>(null)
  const [isLoading, setIsLoading] = useState(true)

  const userRoles = useMemo(() => extractRoles(user?.access_token), [user?.access_token])
  const hasRole = useMemo(() => (role: string) => userRoles.includes(role), [userRoles])

  useEffect(() => {
    userManager.getUser().then(loadedUser => {
      setUser(loadedUser ?? null)
      setIsLoading(false)
    })

    const onUserLoaded = (loadedUser: User) => setUser(loadedUser)
    const onUserUnloaded = () => setUser(null)
    const onAccessTokenExpired = () => {
      userManager.signinSilent().catch(() => {})
    }

    userManager.events.addUserLoaded(onUserLoaded)
    userManager.events.addUserUnloaded(onUserUnloaded)
    userManager.events.addAccessTokenExpired(onAccessTokenExpired)

    return () => {
      userManager.events.removeUserLoaded(onUserLoaded)
      userManager.events.removeUserUnloaded(onUserUnloaded)
      userManager.events.removeAccessTokenExpired(onAccessTokenExpired)
    }
  }, [])

  const login = () => userManager.signinRedirect()
  const logout = () => userManager.signoutRedirect()
  const getToken = async () => (await userManager.getUser())?.access_token ?? null

  return (
    <AuthContext.Provider value={{ user, isLoading, isAuthenticated: !!user, login, logout, getToken, userRoles, hasRole }}>
      {children}
    </AuthContext.Provider>
  )
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth must be used within AuthProvider')
  return ctx
}
