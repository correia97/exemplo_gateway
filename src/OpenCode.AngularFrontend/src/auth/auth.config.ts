import { PassedInitialConfig } from 'angular-auth-oidc-client';
import { KEYCLOAK_URL, DRAGONBALL_API_URL, MUSIC_API_URL } from '../api/env';

export const authConfig: PassedInitialConfig = {
  config: {
    authority: `${KEYCLOAK_URL}/realms/opencode`,
    redirectUrl: window.location.origin + '/callback',
    postLogoutRedirectUri: window.location.origin,
    clientId: 'frontend',
    scope: 'openid profile email roles',
    responseType: 'code',
    silentRenew: true,
    useRefreshToken: true,
    renewTimeBeforeTokenExpiresInSeconds: 30,
    secureRoutes: [DRAGONBALL_API_URL + '/', MUSIC_API_URL + '/'],
  },
};
