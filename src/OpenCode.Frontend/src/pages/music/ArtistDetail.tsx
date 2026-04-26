import type { Artist } from '../../api/types'
import WriteGuard from '../../auth/WriteGuard'

interface Props { artist: Artist; onSelectAlbum: (id: number) => void; onCreateAlbum: () => void; onEdit: () => void; onDelete: () => void; onBack: () => void }

export default function ArtistDetail({ artist, onSelectAlbum, onCreateAlbum, onEdit, onDelete, onBack }: Props) {
  return (
    <div>
      <button onClick={onBack} className="mb-4 px-3 py-1.5 cursor-pointer border border-gray-300 rounded text-sm">&larr; Back to artists</button>
      <div className="flex gap-8 flex-wrap">
        <div className="flex-1 min-w-[280px]">
          <h2 className="text-xl font-bold">{artist.name}</h2>
          <dl className="mt-4 leading-8">
            {artist.genre && <><dt className="font-bold text-gray-600">Genre</dt><dd>{artist.genre}</dd></>}
            {artist.biography && <><dt className="font-bold text-gray-600">Biography</dt><dd>{artist.biography}</dd></>}
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
            <h3 className="text-lg font-semibold">Albums ({artist.albums?.length ?? 0})</h3>
            <WriteGuard>
              <button onClick={onCreateAlbum} className="px-3 py-1.5 bg-indigo-600 text-white rounded text-sm cursor-pointer border-none">+ Add Album</button>
            </WriteGuard>
          </div>
          {artist.albums?.length > 0 ? (
            <div className="flex flex-col gap-2">
              {artist.albums.map(album => (
                <div key={album.id} onClick={() => onSelectAlbum(album.id)}
                  className="p-3 border border-gray-300 rounded-lg cursor-pointer bg-white hover:border-indigo-600 transition-colors">
                  <strong>{album.title}</strong>
                  <span className="ml-4 text-xs text-gray-500">
                    {album.releaseYear} {album.genre ? `· ${album.genre}` : ''} · {album.tracks?.length ?? 0} tracks
                  </span>
                </div>
              ))}
            </div>
          ) : <p className="text-gray-400">No albums yet</p>}
        </div>
      </div>
    </div>
  )
}
