import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { fetchDragonBallStats, fetchMusicStats, type AdminStats } from '../../api/admin'

export default function DashboardPage() {
  const [stats, setStats] = useState<AdminStats | null>(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    async function load() {
      try {
        const [dbStats, musicStats] = await Promise.all([
          fetchDragonBallStats(),
          fetchMusicStats(),
        ])
        setStats({
          characters: dbStats.characters,
          genres: musicStats.genres,
          artists: musicStats.artists,
          albums: musicStats.albums,
          tracks: musicStats.tracks,
        })
      } catch (err) {
        setError('Failed to load admin stats')
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [])

  if (loading) return <div className="p-8 text-center">Loading dashboard...</div>
  if (error) return <div className="p-8 text-center text-red-600">{error}</div>

  const cards = [
    { label: 'Characters', count: stats?.characters ?? 0, path: '/admin/characters', color: 'bg-blue-500' },
    { label: 'Genres', count: stats?.genres ?? 0, path: '/admin/genres', color: 'bg-purple-500' },
    { label: 'Artists', count: stats?.artists ?? 0, path: '/admin/artists', color: 'bg-indigo-500' },
    { label: 'Albums', count: stats?.albums ?? 0, path: '/admin/albums', color: 'bg-pink-500' },
    { label: 'Tracks', count: stats?.tracks ?? 0, path: '/admin/tracks', color: 'bg-teal-500' },
  ]

  return (
    <div>
      <h1 className="text-2xl font-bold mb-6">Admin Dashboard</h1>
      <div className="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
        {cards.map(card => (
          <Link key={card.label} to={card.path}
            className={`${card.color} rounded-lg p-4 text-white hover:opacity-90 transition-opacity`}>
            <div className="text-3xl font-bold">{card.count}</div>
            <div className="text-sm opacity-90 mt-1">{card.label}</div>
          </Link>
        ))}
      </div>
    </div>
  )
}
