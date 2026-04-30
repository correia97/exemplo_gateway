import { useEffect, useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { userManager } from '../auth/config'

export default function Callback() {
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    userManager.signinRedirectCallback()
      .then(() => navigate('/', { replace: true }))
      .catch(err => setError(err.message || 'Authentication failed'))
  }, [navigate])

  if (error) return <div className="p-8 text-red-600">Error: {error}</div>
  return <div className="p-8">Completing login...</div>
}
