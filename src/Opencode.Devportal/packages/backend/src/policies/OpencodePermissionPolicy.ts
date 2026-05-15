import type { PermissionPolicy } from '@backstage/plugin-permission-node';
import type {
  PolicyQuery,
  PolicyQueryUser,
} from '@backstage/plugin-permission-node';
import {
  AuthorizeResult,
  type PolicyDecision,
  type PermissionCondition,
  type PermissionCriteria,
  isResourcePermission,
} from '@backstage/plugin-permission-common';
import {
  catalogEntityCreatePermission,
  catalogEntityReadPermission,
  catalogEntityDeletePermission,
  RESOURCE_TYPE_CATALOG_ENTITY,
} from '@backstage/plugin-catalog-common/alpha';
import { createCatalogConditionalDecision } from '@backstage/plugin-catalog-backend/alpha';

export class OpencodePermissionPolicy implements PermissionPolicy {
  async handle(
    request: PolicyQuery,
    user?: PolicyQueryUser,
  ): Promise<PolicyDecision> {
    const { permission } = request;

    if (permission.name === catalogEntityReadPermission.name) {
      return { result: AuthorizeResult.ALLOW };
    }

    if (permission.name === catalogEntityCreatePermission.name) {
      if (hasOwnershipRefs(user)) {
        return { result: AuthorizeResult.ALLOW };
      }
      return { result: AuthorizeResult.DENY };
    }

    if (permission.name === catalogEntityDeletePermission.name) {
      if (!hasOwnershipRefs(user)) {
        return { result: AuthorizeResult.DENY };
      }

      if (isResourcePermission(permission, RESOURCE_TYPE_CATALOG_ENTITY)) {
        return createCatalogConditionalDecision(
          permission,
          createOwnerCondition(user!),
        );
      }

      return { result: AuthorizeResult.DENY };
    }

    if (hasOwnershipRefs(user)) {
      return { result: AuthorizeResult.ALLOW };
    }

    return { result: AuthorizeResult.DENY };
  }
}

function hasOwnershipRefs(
  user?: PolicyQueryUser,
): user is PolicyQueryUser {
  const refs = user?.identity?.ownershipEntityRefs;
  return !!(refs && refs.length > 0);
}

function createOwnerCondition(
  user: PolicyQueryUser,
): PermissionCriteria<
  PermissionCondition<'catalog-entity', { claims: string[] }>
> {
  return {
    rule: 'isEntityOwner',
    resourceType: RESOURCE_TYPE_CATALOG_ENTITY,
    params: {
      claims: user.identity.ownershipEntityRefs,
    },
  };
}
