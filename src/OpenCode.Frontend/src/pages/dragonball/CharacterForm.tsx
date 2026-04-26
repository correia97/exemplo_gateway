import { useState } from 'react'
import type { CharacterCreatePayload, Character } from '../../api/types'

interface Props {
  onSubmit: (data: CharacterCreatePayload) => Promise<void>
  onCancel: () => void
  mode: 'create' | 'edit'
  initial?: Character
}

export default function CharacterForm({ onSubmit, onCancel, mode, initial }: Props) {
  const [name, setName] = useState(initial?.name ?? '')
  const [race, setRace] = useState(initial?.race ?? '')
  const [ki, setKi] = useState(initial?.ki ?? '')
  const [maxKi, setMaxKi] = useState(initial?.maxKi ?? '')
  const [description, setDescription] = useState(initial?.description ?? '')
  const [planetId, setPlanetId] = useState(initial?.planet?.id ? String(initial.planet.id) : '')
  const [submitting, setSubmitting] = useState(false)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!name.trim() || !race.trim() || !ki.trim()) return
    setSubmitting(true)
    await onSubmit({
      name: name.trim(),
      race: race.trim(),
      ki: ki.trim(),
      maxKi: maxKi.trim(),
      description: description.trim() || undefined,
      planetId: planetId ? Number(planetId) : undefined,
    })
    setSubmitting(false)
  }

  const labelClass = 'block font-semibold mt-3 mb-1'
  const inputClass = 'w-full p-2 border border-gray-300 rounded'

  return (
    <form onSubmit={handleSubmit} className="max-w-[500px]">
      <h2 className="text-xl font-bold mb-4">{mode === 'create' ? 'New Character' : 'Edit Character'}</h2>
      <label className={labelClass}>Name *</label>
      <input className={inputClass} value={name} onChange={e => setName(e.target.value)} required />
      <label className={labelClass}>Race *</label>
      <input className={inputClass} value={race} onChange={e => setRace(e.target.value)} required />
      <label className={labelClass}>Ki *</label>
      <input className={inputClass} value={ki} onChange={e => setKi(e.target.value)} required />
      <label className={labelClass}>Max Ki</label>
      <input className={inputClass} value={maxKi} onChange={e => setMaxKi(e.target.value)} />
      <label className={labelClass}>Description</label>
      <textarea className={`${inputClass} min-h-[80px]`} value={description} onChange={e => setDescription(e.target.value)} />
      <label className={labelClass}>Planet ID</label>
      <input className={inputClass} type="number" value={planetId} onChange={e => setPlanetId(e.target.value)} />
      <div className="flex gap-2 mt-6">
        <button type="submit" disabled={submitting} className="px-6 py-2 bg-blue-600 text-white rounded disabled:opacity-50 disabled:cursor-not-allowed hover:bg-blue-700">
          {submitting ? 'Saving...' : (mode === 'create' ? 'Create' : 'Save Changes')}
        </button>
        <button type="button" onClick={onCancel} className="px-6 py-2 border border-gray-300 rounded cursor-pointer bg-white hover:bg-gray-50">Cancel</button>
      </div>
    </form>
  )
}
