import { Link } from 'react-router-dom'
import { useAuth } from '../../auth/AuthProvider'

export default function Dashboard() {
  const { isAuthenticated } = useAuth()

  if (!isAuthenticated) {
    return (
      <div className="text-center p-16">
        <h1 className="text-3xl font-bold">Welcome to OpenCode</h1>
        <p className="my-4 text-gray-500">
          Browse Dragon Ball characters and Music catalog. Login to create or edit.
        </p>
        <div className="flex gap-4 justify-center mt-8">
          <Link to="/dragonball" className="px-8 py-4 bg-gray-200 rounded-lg block hover:bg-gray-300 transition-colors">
            Browse Characters
          </Link>
          <Link to="/music" className="px-8 py-4 bg-gray-200 rounded-lg block hover:bg-gray-300 transition-colors">
            Browse Music
          </Link>
        </div>
      </div>
    )
  }

  return (
    <div>
      <h1 className="text-2xl font-bold">Dashboard</h1>
      <p className="my-4 text-gray-500">Authenticated — browse or manage data below.</p>
      <div className="flex gap-4 mt-6">
        <Link to="/dragonball" className="p-6 bg-blue-50 rounded-lg block flex-1 hover:bg-blue-100 transition-colors">
          <h3 className="font-semibold">Dragon Ball</h3>
          <p className="text-sm text-gray-500">Characters</p>
        </Link>
        <Link to="/music" className="p-6 bg-purple-50 rounded-lg block flex-1 hover:bg-purple-100 transition-colors">
          <h3 className="font-semibold">Music Catalog</h3>
          <p className="text-sm text-gray-500">Artists, Albums, Tracks</p>
        </Link>
      </div>
    </div>
  )
}
