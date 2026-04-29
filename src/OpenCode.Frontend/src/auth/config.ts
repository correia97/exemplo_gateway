import { UserManager, WebStorageStateStore, type UserManagerSettings } from 'oidc-client-ts'
import { KEYCLOAK_URL } from '../api/client'

//const env = (window as any).__ENV__ || {}
const keycloakUrl = KEYCLOAK_URL
const origin = window.location.origin

const settings: UserManagerSettings = {
  authority: `${keycloakUrl}/realms/OpenCode`,
  client_id: 'frontend',
  redirect_uri: `${origin}/callback`,
  post_logout_redirect_uri: origin,
  response_type: 'code',
  scope: 'openid profile email roles',
  loadUserInfo: true,
  automaticSilentRenew: true,
  silent_redirect_uri: `${origin}/callback`,
  monitorSession: true,
  userStore: new WebStorageStateStore({ store: window.sessionStorage }),
}

export const userManager = new UserManager(settings)

export function extractRoles(accessToken?: string): string[] {
  if (!accessToken) return []
  try {
    const payload = JSON.parse(atob(accessToken.split('.')[1]))
    return payload.realm_access?.roles ?? payload.roles ?? []
  } catch { return [] }
}

export async function getAccessToken(): Promise<string | null> {
  const user = await userManager.getUser()
  return user?.access_token ?? null
}
