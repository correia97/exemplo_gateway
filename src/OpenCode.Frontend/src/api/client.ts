import { getAccessToken } from '../auth/config'

const BASE_URL = 'http://localhost:9080/api'

class ApiClientError extends Error {
  correlationId?: string
  statusCode: number
  
  constructor(message: string, statusCode: number, correlationId?: string) {
    super(message)
    this.name = 'ApiClientError'
    this.statusCode = statusCode
    this.correlationId = correlationId
  }
}

async function apiFetch<T>(
  path: string,
  options: RequestInit = {}
): Promise<T> {
  const token = await getAccessToken()
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  }
  if (token) headers['Authorization'] = `Bearer ${token}`

  const response = await fetch(`${BASE_URL}${path}`, { ...options, headers })
  const correlationId = response.headers.get('X-Correlation-Id') ?? undefined

  if (!response.ok) {
    const body = await response.json().catch(() => ({}))
    const message = body.message || body.title || `Request failed (${response.status})`
    throw new ApiClientError(message, response.status, correlationId)
  }

  if (response.status === 204) return undefined as T
  return response.json()
}

export { ApiClientError }
export default apiFetch
