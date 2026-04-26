import { useState, type ReactNode } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../auth/AuthProvider'

export default function Layout({ children }: { children: ReactNode }) {
  const { isAuthenticated, user, userRoles, login, logout } = useAuth()
  const [sidebarOpen, setSidebarOpen] = useState(true)

  return (
    <div className="min-h-screen flex flex-col">
      <header className="bg-slate-900 text-white h-14 flex items-center px-6 gap-4 shrink-0">
        <button
          onClick={() => setSidebarOpen(!sidebarOpen)}
          className="text-white p-1 hover:bg-slate-700 rounded cursor-pointer"
          aria-label={sidebarOpen ? 'Collapse sidebar' : 'Expand sidebar'}
        >
          <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
          </svg>
        </button>
        <Link to="/" className="text-white font-bold text-lg hover:text-slate-200">
          OpenCode
        </Link>
        <div className="ml-auto flex items-center gap-3">
          {isAuthenticated ? (
            <>
              <span className="text-sm text-slate-300">{user?.profile?.preferred_username}</span>
              {userRoles.length > 0 && (
                <span className="text-xs bg-slate-700 text-slate-300 px-2 py-0.5 rounded">
                  {userRoles.join(', ')}
                </span>
              )}
              <button
                onClick={logout}
                className="bg-transparent text-white border border-white/30 px-3 py-1 rounded cursor-pointer text-sm hover:bg-slate-700 transition-colors"
              >
                Logout
              </button>
            </>
          ) : (
            <button
              onClick={login}
              className="bg-blue-600 text-white border-none px-3 py-1 rounded cursor-pointer text-sm hover:bg-blue-700 transition-colors"
            >
              Login
            </button>
          )}
        </div>
      </header>

      <div className="flex flex-1">
        {sidebarOpen && (
          <aside className="w-56 bg-slate-800 text-white p-4 shrink-0">
            <nav className="flex flex-col gap-1">
              <Link to="/" className="block px-3 py-2 rounded text-slate-300 hover:bg-slate-700 hover:text-white text-sm transition-colors">
                Dashboard
              </Link>
              <Link to="/dragonball" className="block px-3 py-2 rounded text-slate-300 hover:bg-slate-700 hover:text-white text-sm transition-colors">
                Dragon Ball
              </Link>
              <Link to="/music" className="block px-3 py-2 rounded text-slate-300 hover:bg-slate-700 hover:text-white text-sm transition-colors">
                Music
              </Link>
            </nav>
          </aside>
        )}
        <main className="flex-1 p-6 max-w-6xl w-full mx-auto">
          {children}
        </main>
      </div>
    </div>
  )
}
