declare global {
  interface Window {
    __ENV__?: Record<string, string>
  }
}

const env = window.__ENV__ || {}

export const DRAGONBALL_API_URL = env.DRAGONBALL_API_URL ?? 'http://localhost:5000'
export const MUSIC_API_URL = env.MUSIC_API_URL ?? 'http://localhost:5002'
export const KEYCLOAK_URL = env.KEYCLOAK_URL ?? 'http://localhost:8080'
