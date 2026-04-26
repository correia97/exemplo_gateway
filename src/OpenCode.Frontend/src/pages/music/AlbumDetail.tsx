import type { Album } from '../../api/types'
import WriteGuard from '../../auth/WriteGuard'

interface Props { album: Album; onCreateTrack: () => void; onEdit: () => void; onDelete: () => void; onBack: () => void }

export default function AlbumDetail({ album, onCreateTrack, onEdit, onDelete, onBack }: Props) {
  const formatDuration = (s?: number) => {
    if (!s) return '—'
    const m = Math.floor(s / 60)
    const sec = s % 60
    return `${m}:${String(sec).padStart(2, '0')}`
  }

  return (
    <div>
      <button onClick={onBack} className="mb-4 px-3 py-1.5 cursor-pointer border border-gray-300 rounded text-sm">&larr; Back to artist</button>
      <div className="flex gap-8 flex-wrap">
        <div className="flex-1 min-w-[280px]">
          <h2 className="text-xl font-bold">{album.title}</h2>
          <dl className="mt-4 leading-8">
            {album.artistName && <><dt className="font-bold text-gray-600">Artist</dt><dd>{album.artistName}</dd></>}
            {album.releaseYear && <><dt className="font-bold text-gray-600">Release Year</dt><dd>{album.releaseYear}</dd></>}
            {album.genre && <><dt className="font-bold text-gray-600">Genre</dt><dd>{album.genre}</dd></>}
          </dl>
          <WriteGuard>
            <div className="flex gap-2 mt-6">
              <button onClick={onEdit} className="px-4 py-2 bg-indigo-600 text-white rounded cursor-pointer border-none">Edit</button>
              <button onClick={onDelete} className="px-4 py-2 bg-red-600 text-white rounded cursor-pointer border-none">Delete</button>
            </div>
          </WriteGuard>
        </div>
        <div className="flex-[2] min-w-[300px]">
          <div className="flex justify-between items-center mb-3">
            <h3 className="text-lg font-semibold">Tracks ({album.tracks?.length ?? 0})</h3>
            <WriteGuard>
              <button onClick={onCreateTrack} className="px-3 py-1.5 bg-indigo-600 text-white rounded text-sm cursor-pointer border-none">+ Add Track</button>
            </WriteGuard>
          </div>
          {album.tracks?.length > 0 ? (
            <table className="w-full border-collapse text-sm">
              <thead>
                <tr className="bg-gray-100 text-left">
                  <th className="p-2 border-b-2 border-gray-300">#</th>
                  <th className="p-2 border-b-2 border-gray-300">Title</th>
                  <th className="p-2 border-b-2 border-gray-300">Duration</th>
                </tr>
              </thead>
              <tbody>
                {album.tracks.map(track => (
                  <tr key={track.id} className="border-b border-gray-200">
                    <td className="p-2 text-gray-400">{track.trackNumber ?? '—'}</td>
                    <td className="p-2">{track.title}</td>
                    <td className="p-2">{formatDuration(track.duration)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          ) : <p className="text-gray-400">No tracks yet</p>}
        </div>
      </div>
    </div>
  )
}
