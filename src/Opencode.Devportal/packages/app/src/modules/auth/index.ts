import { createFrontendModule } from '@backstage/frontend-plugin-api';
import { oidcAuthApi } from './apis';
import { oidcSignInPage } from './SignInPage';

export const authModule = createFrontendModule({
  pluginId: 'app',
  extensions: [oidcAuthApi, oidcSignInPage],
});
