import React, { useEffect, useState } from 'react';
import {
  Content,
  Header,
  Page,
  Progress,
  ResponseErrorPanel,
  InfoCard,
} from '@backstage/core-components';
import { Alert, AlertTitle } from '@material-ui/lab';
import {
  Button,
  FormControl,
  Grid,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  makeStyles,
} from '@material-ui/core';
import {
  useApi,
  identityApiRef,
  discoveryApiRef,
  fetchApiRef,
} from '@backstage/core-plugin-api';
import { useNavigate, useParams } from 'react-router-dom';
import type { Entity } from '@backstage/catalog-model';
import { catalogApiRef } from '@backstage/plugin-catalog-react';

const useStyles = makeStyles(theme => ({
  form: {
    maxWidth: 800,
  },
  field: {
    marginBottom: theme.spacing(2),
  },
  actions: {
    marginTop: theme.spacing(3),
    display: 'flex',
    gap: theme.spacing(2),
  },
}));

const LIFECYCLES = ['experimental', 'production', 'deprecated'] as const;

export function ApiEditPage() {
  const classes = useStyles();
  const catalogApi = useApi(catalogApiRef);
  const identityApi = useApi(identityApiRef);
  const discoveryApi = useApi(discoveryApiRef);
  const { fetch } = useApi(fetchApiRef);
  const navigate = useNavigate();
  const { kind, namespace, name } = useParams<{
    kind: string;
    namespace: string;
    name: string;
  }>();

  const [entity, setEntity] = useState<Entity | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  const [description, setDescription] = useState('');
  const [lifecycle, setLifecycle] = useState('experimental');
  const [owner, setOwner] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  useEffect(() => {
    async function loadEntity() {
      try {
        const entityRef = `${kind}:${namespace}/${name}`;
        const e = await catalogApi.getEntityByRef(entityRef);
        if (!e) {
          setError(new Error(`Entity ${entityRef} not found`));
          return;
        }
        setEntity(e);
        setDescription(e.metadata.description || '');
        setLifecycle((e.spec?.lifecycle as string) || 'experimental');
        setOwner((e.spec?.owner as string) || '');
      } catch (err: any) {
        setError(err);
      } finally {
        setLoading(false);
      }
    }
    loadEntity();
  }, [kind, namespace, name, catalogApi]);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!entity) return;
    setSubmitError(null);
    setSubmitting(true);

    try {
      const identity = await identityApi.getBackstageIdentity();
      const ownerRefs = identity.ownershipEntityRefs ?? [];
      const currentOwner = entity.spec?.owner as string | undefined;

      if (
        currentOwner &&
        !ownerRefs.some((ref: string) => ref === currentOwner)
      ) {
        setSubmitError('You are not the owner of this API');
        setSubmitting(false);
        return;
      }

      const updated: Entity = {
        ...entity,
        metadata: {
          ...entity.metadata,
          description: description || undefined,
        },
        spec: {
          ...entity.spec,
          type: entity.spec?.type,
          lifecycle,
          owner: owner || entity.spec?.owner,
        },
      };

      const baseUrl = await discoveryApi.getBaseUrl('opencode-entity-api');
      const response = await fetch(
        `${baseUrl}/entities/${kind}/${namespace}/${name}`,
        {
          method: 'PUT',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(updated),
        },
      );

      if (!response.ok) {
        const errData = await response.json().catch(() => ({}));
        throw new Error(
          errData.error || `Server error: ${response.status}`,
        );
      }

      setSuccess(true);
      setTimeout(() => {
        navigate(`/catalog/${kind}/${namespace}/${name}`);
      }, 1500);
    } catch (err: any) {
      setSubmitError(err?.message || 'Failed to update API');
    } finally {
      setSubmitting(false);
    }
  };

  if (loading) return <Progress />;
  if (error) return <ResponseErrorPanel error={error} />;

  return (
    <Page themeId="tool">
      <Header
        title={`Edit API: ${entity?.metadata.name ?? name}`}
        subtitle="Modify your registered API"
      />
      <Content>
        {success ? (
          <Alert severity="success">
            <AlertTitle>API Updated Successfully</AlertTitle>
            Redirecting to API details...
          </Alert>
        ) : (
          <InfoCard title="Edit API Details">
            <form className={classes.form} onSubmit={handleSave}>
              {submitError && (
                <Alert severity="error" className={classes.field}>
                  {submitError}
                </Alert>
              )}

              <TextField
                className={classes.field}
                label="Name"
                value={entity?.metadata.name ?? ''}
                disabled
                fullWidth
                helperText="API name cannot be changed"
              />

              <TextField
                className={classes.field}
                label="Description"
                value={description}
                onChange={e => setDescription(e.target.value)}
                fullWidth
                multiline
                rows={2}
              />

              <Grid container spacing={2}>
                <Grid item xs={6}>
                  <TextField
                    className={classes.field}
                    label="API Type"
                    value={entity?.spec?.type ?? ''}
                    disabled
                    fullWidth
                  />
                </Grid>
                <Grid item xs={6}>
                  <FormControl className={classes.field} fullWidth>
                    <InputLabel>Lifecycle</InputLabel>
                    <Select
                      value={lifecycle}
                      onChange={e =>
                        setLifecycle(e.target.value as string)
                      }
                      label="Lifecycle"
                    >
                      {LIFECYCLES.map(l => (
                        <MenuItem key={l} value={l}>
                          {l}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
                </Grid>
              </Grid>

              <TextField
                className={classes.field}
                label="Owner"
                value={owner}
                onChange={e => setOwner(e.target.value)}
                fullWidth
                helperText="Entity reference (e.g. user:default/dev or group:default/guests)"
              />

              <div className={classes.actions}>
                <Button
                  variant="contained"
                  color="primary"
                  type="submit"
                  disabled={submitting}
                >
                  {submitting ? 'Saving...' : 'Save Changes'}
                </Button>
                <Button
                  variant="outlined"
                  onClick={() => navigate('/my-apis')}
                >
                  Cancel
                </Button>
              </div>
            </form>
          </InfoCard>
        )}
      </Content>
    </Page>
  );
}
