import { SignInPageBlueprint } from '@backstage/plugin-app-react';
import { oidcAuthApiRef } from './apis';

export const oidcSignInPage = SignInPageBlueprint.make({
  params: {
    loader: async () => {
      const { SignInPage } = await import('@backstage/core-components');
      return props => (
        <SignInPage
          {...props}
          providers={[
            {
              id: 'oidc',
              title: 'Keycloak',
              message: 'Sign in via Keycloak',
              apiRef: oidcAuthApiRef,
            },
          ]}
        />
      );
    },
  },
});
