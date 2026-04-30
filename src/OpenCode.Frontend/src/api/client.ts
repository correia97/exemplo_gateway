import { getAccessToken } from '../auth/config'

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

const env = (window as any).__ENV__ || {}

export const APISIX_URL = import.meta.env.VITE_APISIX_URL as string | undefined
  ?? env.APISIX_URL
  ?? 'http://localhost:9080'

export const KEYCLOAK_URL = import.meta.env.VITE_KEYCLOAK_URL as string | undefined
  ?? env.KEYCLOAK_URL
  ?? 'http://localhost:8080'

async function apiFetch<T>(
  url: string,
  options: RequestInit = {}
): Promise<T> {
  const token = await getAccessToken()
  const headers: Record<string, string> = {
    'Content-Type': 'application/json',
    ...(options.headers as Record<string, string>),
  }
  if (token) headers['Authorization'] = `Bearer ${token}`

  const fullUrl = url.startsWith('http') ? url : `${APISIX_URL}${url}`
  const response = await fetch(fullUrl, { ...options, headers })
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
