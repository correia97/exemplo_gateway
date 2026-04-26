import type { Character } from '../../api/types'
import WriteGuard from '../../auth/WriteGuard'

interface Props { character: Character; onEdit: () => void; onDelete: () => void; onBack: () => void }

export default function CharacterDetail({ character, onEdit, onDelete, onBack }: Props) {
  return (
    <div>
      <button onClick={onBack} className="mb-4 px-3 py-1.5 cursor-pointer border border-gray-300 rounded bg-white hover:bg-gray-50">&larr; Back to list</button>
      <div className="flex gap-8 flex-wrap">
        <div className="flex-1 min-w-[280px]">
          <h2 className="text-xl font-bold">{character.name}</h2>
          <dl className="mt-4 leading-8">
            <dt className="font-bold text-gray-600">Race</dt><dd>{character.race}</dd>
            <dt className="font-bold text-gray-600">Ki</dt><dd>{character.ki}</dd>
            <dt className="font-bold text-gray-600">Max Ki</dt><dd>{character.maxKi}</dd>
            {character.planet && <><dt className="font-bold text-gray-600">Planet</dt><dd>{character.planet.name}</dd></>}
            {character.description && <><dt className="font-bold text-gray-600">Description</dt><dd>{character.description}</dd></>}
          </dl>
          <WriteGuard>
            <div className="flex gap-2 mt-6">
              <button onClick={onEdit} className="px-4 py-2 bg-blue-600 text-white rounded cursor-pointer hover:bg-blue-700">Edit</button>
              <button onClick={onDelete} className="px-4 py-2 bg-red-600 text-white rounded cursor-pointer hover:bg-red-700">Delete</button>
            </div>
          </WriteGuard>
        </div>
        <div className="flex-1 min-w-[240px]">
          <h3 className="text-lg font-semibold">Transformations ({character.transformations?.length ?? 0})</h3>
          {character.transformations?.length > 0 ? (
            <ul className="mt-3 list-none">
              {character.transformations.map(t => (
                <li key={t.id} className="py-2 border-b border-gray-200">
                  <strong>{t.name}</strong> — Ki: {t.ki}
                  {t.description && <p className="text-sm text-gray-500">{t.description}</p>}
                </li>
              ))}
            </ul>
          ) : <p className="text-gray-400">No transformations</p>}
        </div>
      </div>
    </div>
  )
}
