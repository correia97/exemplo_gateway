import { PassedInitialConfig } from 'angular-auth-oidc-client';
import { KEYCLOAK_URL } from '../api/env';

export const authConfig: PassedInitialConfig = {
  config: {
    authority: `${KEYCLOAK_URL}/realms/OpenCode`,
    redirectUrl: window.location.origin + '/callback',
    postLogoutRedirectUri: window.location.origin,
    clientId: 'frontend',
    scope: 'openid profile email roles',
    responseType: 'code',
    silentRenew: true,
    useRefreshToken: true,    
    renewTimeBeforeTokenExpiresInSeconds: 30,
  },
};
