import { Link, Outlet, useLocation } from 'react-router-dom'

const navItems = [
  { label: 'Dashboard', path: '/admin' },
  { type: 'section' as const, label: 'Dragon Ball' },
  { label: 'Characters', path: '/admin/characters' },
  { type: 'section' as const, label: 'Music' },
  { label: 'Genres', path: '/admin/genres' },
  { label: 'Artists', path: '/admin/artists' },
  { label: 'Albums', path: '/admin/albums' },
  { label: 'Tracks', path: '/admin/tracks' },
]

export default function AdminLayout() {
  const location = useLocation()

  return (
    <div className="flex min-h-[calc(100vh-4rem)]">
      <aside className="w-64 bg-gray-900 text-white p-4 shrink-0">
        <h2 className="text-lg font-bold mb-4 px-2">Admin Panel</h2>
        <nav className="space-y-1">
          {navItems.map(item => {
            if ('type' in item && item.type === 'section') {
              return (
                <div key={item.label} className="text-xs uppercase tracking-wider text-gray-400 mt-4 mb-1 px-2">
                  {item.label}
                </div>
              )
            }
            const isActive = location.pathname === item.path!
            return (
              <Link key={item.path} to={item.path!}
                className={`flex items-center gap-2 px-3 py-2 rounded text-sm transition-colors ${
                  isActive ? 'bg-blue-600 text-white' : 'text-gray-300 hover:bg-gray-800'
                }`}>
                {item.label}
              </Link>
            )
          })}
        </nav>
      </aside>
      <main className="flex-1 p-6 bg-gray-50">
        <Outlet />
      </main>
    </div>
  )
}
