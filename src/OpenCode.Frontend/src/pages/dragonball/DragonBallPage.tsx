import { useState, useCallback } from 'react'
import CharacterList from './CharacterList'
import CharacterDetail from './CharacterDetail'
import CharacterForm from './CharacterForm'
import { useApiError } from '../../hooks/useApiError'
import type { Character, CharacterCreatePayload } from '../../api/types'
import { getCharacter, createCharacter, updateCharacter, deleteCharacter } from '../../api/dragonball'

type View = 'list' | 'detail' | 'create' | 'edit'

export default function DragonBallPage() {
  const [view, setView] = useState<View>('list')
  const [character, setCharacter] = useState<Character | null>(null)
  const { toasts, handleError, dismissToast } = useApiError()

  const handleSelect = useCallback(async (id: number) => {
    try {
      const c = await getCharacter(id)
      setCharacter(c)
      setView('detail')
    } catch (e: unknown) {
      handleError(e)
    }
  }, [handleError])

  const handleCreate = useCallback(async (data: CharacterCreatePayload) => {
    try {
      await createCharacter(data)
      setView('list')
    } catch (e: unknown) {
      handleError(e)
    }
  }, [handleError])

  const handleUpdate = useCallback(async (id: number, data: Partial<CharacterCreatePayload>) => {
    try {
      const updated = await updateCharacter(id, data)
      setCharacter(updated)
      setView('detail')
    } catch (e: unknown) {
      handleError(e)
    }
  }, [handleError])

  const handleDelete = useCallback(async (id: number) => {
    if (!window.confirm('Delete this character?')) return
    try {
      await deleteCharacter(id)
      setView('list')
    } catch (e: unknown) {
      handleError(e)
    }
  }, [handleError])

  const handleCancel = useCallback(() => {
    setView('list')
  }, [])

  return (
    <div>
      <h1 className="text-2xl font-bold mb-4">Dragon Ball Characters</h1>

      {toasts.length > 0 && (
        <div className="fixed top-4 right-4 z-50 flex flex-col gap-2">
          {toasts.map(t => (
            <div
              key={t.id}
              className="px-4 py-3 rounded shadow-lg text-white bg-red-600 flex items-center gap-3 min-w-[300px]"
            >
              <span className="flex-1 text-sm">{t.message}</span>
              {t.correlationId && <span className="text-xs text-red-200 font-mono">{t.correlationId}</span>}
              <button onClick={() => dismissToast(t.id)} className="text-white/80 hover:text-white text-lg leading-none">&times;</button>
            </div>
          ))}
        </div>
      )}

      {view === 'list' && (
        <CharacterList onSelect={handleSelect} onCreate={() => setView('create')} />
      )}
      {view === 'detail' && character && (
        <CharacterDetail
          character={character}
          onEdit={() => setView('edit')}
          onDelete={() => handleDelete(character.id)}
          onBack={() => setView('list')}
        />
      )}
      {view === 'create' && (
        <CharacterForm onSubmit={handleCreate} onCancel={handleCancel} mode="create" />
      )}
      {view === 'edit' && character && (
        <CharacterForm
          onSubmit={(data: CharacterCreatePayload) => handleUpdate(character.id, data)}
          onCancel={handleCancel}
          mode="edit"
          initial={character}
        />
      )}
    </div>
  )
}
