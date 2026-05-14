import { createFrontendPlugin, PageBlueprint } from '@backstage/frontend-plugin-api';

export const apiRegistrationPage = PageBlueprint.make({
  name: 'register',
  params: {
    path: '/api-registration',
    title: 'Register API',
    loader: () =>
      import('./components/ApiRegistrationPage').then(m => (
        <m.ApiRegistrationPage />
      )),
  },
});

export const apiEditPage = PageBlueprint.make({
  name: 'edit',
  params: {
    path: '/api-registration/edit/:kind/:namespace/:name',
    loader: () =>
      import('./components/ApiEditPage').then(m => <m.ApiEditPage />),
  },
});

export const apiManagementPage = PageBlueprint.make({
  name: 'manage',
  params: {
    path: '/my-apis',
    title: 'My APIs',
    loader: () =>
      import('./components/ApiManagementPage').then(m => (
        <m.ApiManagementPage />
      )),
  },
});

export const apiRegistrationPlugin = createFrontendPlugin({
  pluginId: 'opencode-api-registration',
  extensions: [apiRegistrationPage, apiEditPage, apiManagementPage],
});

export { ApiRegistrationPage } from './components/ApiRegistrationPage';
export { ApiManagementPage } from './components/ApiManagementPage';
export { ApiEditPage } from './components/ApiEditPage';
