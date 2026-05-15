import {
  coreServices,
  createBackendPlugin,
} from '@backstage/backend-plugin-api';
import { createRouter } from './router';

export const opencodeEntityApiPlugin = createBackendPlugin({
  pluginId: 'opencode-entity-api',
  register(env) {
    env.registerInit({
      deps: {
        http: coreServices.httpRouter,
        logger: coreServices.logger,
        config: coreServices.rootConfig,
        discovery: coreServices.discovery,
        auth: coreServices.auth,
        httpAuth: coreServices.httpAuth,
      },
      async init({ http, logger, config, discovery, auth, httpAuth }) {
        const router = await createRouter({
          logger,
          config,
          discovery,
          auth,
          httpAuth,
        });
        http.use(router);
      },
    });
  },
});

export default opencodeEntityApiPlugin;
