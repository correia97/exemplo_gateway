import { UserManager, WebStorageStateStore, type UserManagerSettings } from 'oidc-client-ts'

const settings: UserManagerSettings = {
  authority: 'http://localhost:8080/realms/opencode',
  client_id: 'frontend',
  redirect_uri: 'http://localhost:5173/callback',
  post_logout_redirect_uri: 'http://localhost:5173',
  response_type: 'code',
  scope: 'openid profile email roles realm_roles',
  loadUserInfo: true,
  automaticSilentRenew: true,
  silent_redirect_uri: 'http://localhost:5173/callback',
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
