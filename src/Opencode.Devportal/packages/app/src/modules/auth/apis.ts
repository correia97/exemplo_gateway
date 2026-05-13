import {
  ApiBlueprint,
  configApiRef,
  createApiRef,
  discoveryApiRef,
  oauthRequestApiRef,
} from '@backstage/frontend-plugin-api';
import { OAuth2 } from '@backstage/core-app-api';
import type {
  BackstageIdentityApi,
  OAuthApi,
  OpenIdConnectApi,
  ProfileInfoApi,
  SessionApi,
} from '@backstage/core-plugin-api';

// Custom API ref for the Keycloak/OIDC provider. There's no built-in
// `oidcAuthApiRef`, so we register our own and back it with an `OAuth2`
// client that targets the `oidc` provider configured in the auth backend.
export const oidcAuthApiRef = createApiRef<
  OAuthApi &
    OpenIdConnectApi &
    ProfileInfoApi &
    BackstageIdentityApi &
    SessionApi
>({
  id: 'auth.oidc',
});

export const oidcAuthApi = ApiBlueprint.make({
  name: 'oidc',
  params: defineParams =>
    defineParams({
      api: oidcAuthApiRef,
      deps: {
        discoveryApi: discoveryApiRef,
        oauthRequestApi: oauthRequestApiRef,
        configApi: configApiRef,
      },
      factory: ({ discoveryApi, oauthRequestApi, configApi }) =>
        OAuth2.create({
          configApi,
          discoveryApi,
          oauthRequestApi,
          provider: {
            id: 'oidc',
            title: 'Keycloak',
            icon: () => null,
          },
          defaultScopes: ['openid', 'profile', 'email'],
          environment: configApi.getOptionalString('auth.environment'),
        }),
    }),
});
