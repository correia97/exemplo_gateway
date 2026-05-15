import express, { Router } from 'express';
import { type LoggerService } from '@backstage/backend-plugin-api';
import { type Config } from '@backstage/config';
import {
  type DiscoveryService,
  type AuthService,
  type HttpAuthService,
} from '@backstage/backend-plugin-api';

export interface RouterOptions {
  logger: LoggerService;
  config: Config;
  discovery: DiscoveryService;
  auth: AuthService;
  httpAuth: HttpAuthService;
}

export async function createRouter(
  options: RouterOptions,
): Promise<Router> {
  const { logger, discovery, auth, httpAuth } = options;

  const router = Router();
  router.use(express.json());

  router.post('/entities', async (req, res) => {
    try {
      const credentials = await httpAuth.credentials(req);
      const { token } = await auth.getPluginRequestToken({
        onBehalfOf: credentials,
        targetPluginId: 'catalog',
      });

      const entity = req.body;

      if (!entity || !entity.kind || !entity.metadata?.name) {
        res.status(400).json({
          error: 'Invalid entity: kind and metadata.name are required',
        });
        return;
      }

      const yaml = toYaml(entity);

      const catalogBaseUrl = await discovery.getBaseUrl('catalog');
      const response = await fetch(
        `${catalogBaseUrl}/locations?dryRun=false`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token}`,
          },
          body: JSON.stringify({
            type: 'url',
            target: `data:text/yaml;base64,${Buffer.from(yaml).toString('base64')}`,
          }),
        },
      );

      if (!response.ok) {
        const errBody = await response.text();
        logger.error(`Catalog location creation failed: ${errBody}`);
        res.status(response.status).json({
          error: `Failed to create entity: ${errBody}`,
        });
        return;
      }

      const result = await response.json();
      logger.info(`Entity created: ${entity.kind}:${entity.metadata.namespace || 'default'}/${entity.metadata.name}`);
      res.status(201).json(result);
    } catch (err: any) {
      logger.error(`Entity creation error: ${err.message}`);
      res.status(500).json({ error: err.message || 'Internal server error' });
    }
  });

  router.put('/entities/:kind/:namespace/:name', async (req, res) => {
    try {
      const credentials = await httpAuth.credentials(req);
      const { token } = await auth.getPluginRequestToken({
        onBehalfOf: credentials,
        targetPluginId: 'catalog',
      });

      const { kind, namespace, name } = req.params;
      const entity = req.body;

      if (!entity) {
        res.status(400).json({ error: 'Entity data is required' });
        return;
      }

      entity.kind = kind;
      entity.metadata = entity.metadata || {};
      entity.metadata.namespace = namespace;
      entity.metadata.name = name;

      const yaml = toYaml(entity);

      const catalogBaseUrl = await discovery.getBaseUrl('catalog');
      const response = await fetch(
        `${catalogBaseUrl}/locations?dryRun=false`,
        {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            Authorization: `Bearer ${token}`,
          },
          body: JSON.stringify({
            type: 'url',
            target: `data:text/yaml;base64,${Buffer.from(yaml).toString('base64')}`,
            onConflict: 'refresh',
          }),
        },
      );

      if (!response.ok) {
        const errBody = await response.text();
        logger.error(`Catalog entity update failed: ${errBody}`);
        res.status(response.status).json({
          error: `Failed to update entity: ${errBody}`,
        });
        return;
      }

      const result = await response.json();
      logger.info(`Entity updated: ${kind}:${namespace}/${name}`);
      res.status(200).json(result);
    } catch (err: any) {
      logger.error(`Entity update error: ${err.message}`);
      res.status(500).json({ error: err.message || 'Internal server error' });
    }
  });

  return router;
}

function toYaml(entity: any): string {
  const lines: string[] = [];
  lines.push(`apiVersion: ${entity.apiVersion || 'backstage.io/v1alpha1'}`);
  lines.push(`kind: ${entity.kind}`);

  lines.push('metadata:');
  if (entity.metadata) {
    lines.push(`  name: ${entity.metadata.name}`);
    if (entity.metadata.namespace && entity.metadata.namespace !== 'default') {
      lines.push(`  namespace: ${entity.metadata.namespace}`);
    }
    if (entity.metadata.description) {
      lines.push(`  description: "${escapeYaml(entity.metadata.description)}"`);
    }
    if (entity.metadata.tags && entity.metadata.tags.length > 0) {
      lines.push('  tags:');
      for (const tag of entity.metadata.tags) {
        lines.push(`    - ${tag}`);
      }
    }
    if (entity.metadata.annotations) {
      lines.push('  annotations:');
      for (const [key, value] of Object.entries(entity.metadata.annotations)) {
        lines.push(`    ${key}: "${escapeYaml(String(value))}"`);
      }
    }
  }

  lines.push('spec:');
  if (entity.spec) {
    if (entity.spec.type) lines.push(`  type: ${entity.spec.type}`);
    if (entity.spec.lifecycle) lines.push(`  lifecycle: ${entity.spec.lifecycle}`);
    if (entity.spec.owner) lines.push(`  owner: ${entity.spec.owner}`);
    if (entity.spec.system) lines.push(`  system: ${entity.spec.system}`);
    if (entity.spec.definition) {
      lines.push('  definition: |');
      for (const defLine of String(entity.spec.definition).split('\n')) {
        lines.push(`    ${defLine}`);
      }
    }
  }

  return lines.join('\n') + '\n';
}

function escapeYaml(value: string): string {
  return value.replace(/\\/g, '\\\\').replace(/"/g, '\\"');
}
