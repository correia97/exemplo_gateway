import { useAuth } from '../auth/AuthProvider'
import { Navigate } from 'react-router-dom'

export default function Login() {
  const { login, isAuthenticated, isLoading } = useAuth()
  if (isLoading) return <div className="p-4">Loading...</div>
  if (isAuthenticated) return <Navigate to="/" replace />
  return (
    <div className="text-center p-16">
      <h1 className="text-3xl font-bold">OpenCode</h1>
      <p className="my-4 text-gray-500">Dragon Ball & Music APIs Explorer</p>
      <button onClick={login} className="px-8 py-3 text-lg cursor-pointer bg-blue-600 text-white rounded-lg border-none hover:bg-blue-700 transition-colors">
        Login with Keycloak
      </button>
    </div>
  )
}
