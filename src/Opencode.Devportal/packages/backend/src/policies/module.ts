import { createBackendModule } from '@backstage/backend-plugin-api';
import { policyExtensionPoint } from '@backstage/plugin-permission-node/alpha';
import { OpencodePermissionPolicy } from './OpencodePermissionPolicy';

export const opencodePermissionPolicyModule = createBackendModule({
  pluginId: 'permission',
  moduleId: 'opencode-policy',
  register(reg) {
    reg.registerInit({
      deps: {
        policy: policyExtensionPoint,
      },
      async init({ policy }) {
        policy.setPolicy(new OpencodePermissionPolicy());
      },
    });
  },
});

export default opencodePermissionPolicyModule;
