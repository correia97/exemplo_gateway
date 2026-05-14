import React, { useState } from 'react';
import {
  Content,
  Header,
  Page,
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
  Typography,
  makeStyles,
} from '@material-ui/core';
import {
  useApi,
  identityApiRef,
  discoveryApiRef,
  fetchApiRef,
} from '@backstage/core-plugin-api';
import { useNavigate } from 'react-router-dom';

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

const API_TYPES = ['openapi', 'grpc', 'asyncapi', 'graphql'] as const;
const LIFECYCLES = ['experimental', 'production', 'deprecated'] as const;

export function ApiRegistrationPage() {
  const classes = useStyles();
  const identityApi = useApi(identityApiRef);
  const discoveryApi = useApi(discoveryApiRef);
  const { fetch } = useApi(fetchApiRef);
  const navigate = useNavigate();

  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [apiType, setApiType] = useState<string>('openapi');
  const [lifecycle, setLifecycle] = useState<string>('experimental');
  const [specDefinition, setSpecDefinition] = useState('');
  const [system, setSystem] = useState('opencode-platform');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setSubmitting(true);

    try {
      const identity = await identityApi.getBackstageIdentity();
      const ownerRef =
        identity.ownershipEntityRefs?.[0] ?? identity.userEntityRef;

      const entityName = name.toLowerCase().replace(/[^a-z0-9-]+/g, '-').replace(/^-|-$/g, '');

      let definition = undefined;
      if (specDefinition.trim()) {
        try {
          definition = JSON.parse(specDefinition);
        } catch {
          setError('Invalid JSON in API specification');
          setSubmitting(false);
          return;
        }
      }

      const entity = {
        apiVersion: 'backstage.io/v1alpha1',
        kind: 'API',
        metadata: {
          name: entityName,
          description: description || undefined,
          tags: ['opencode', 'user-registered'],
          annotations: definition
            ? {
                'backstage.io/view-url': `/catalog/default/api/${entityName}`,
              }
            : undefined,
        },
        spec: {
          type: apiType,
          lifecycle,
          owner: ownerRef,
          system: system || undefined,
          definition: definition ? JSON.stringify(definition) : undefined,
        },
      };

      const baseUrl = await discoveryApi.getBaseUrl('opencode-entity-api');
      const response = await fetch(`${baseUrl}/entities`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(entity),
      });

      if (!response.ok) {
        const errData = await response.json().catch(() => ({}));
        throw new Error(
          errData.error || `Server error: ${response.status}`,
        );
      }

      setSuccess(true);
      setTimeout(() => {
        navigate(`/catalog/default/api/${entityName}`);
      }, 1500);
    } catch (err: any) {
      setError(err?.message || 'Failed to register API');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Page themeId="tool">
      <Header
        title="Register New API"
        subtitle="Add a new API to the OpenCode platform catalog"
      />
      <Content>
        {success ? (
          <Alert severity="success">
            <AlertTitle>API Registered Successfully</AlertTitle>
            Redirecting to the API details page...
          </Alert>
        ) : (
          <InfoCard title="API Details">
            <form className={classes.form} onSubmit={handleSubmit}>
              {error && (
                <Alert severity="error" className={classes.field}>
                  {error}
                </Alert>
              )}

              <TextField
                className={classes.field}
                label="API Name"
                placeholder="my-service-api"
                value={name}
                onChange={e => setName(e.target.value)}
                required
                fullWidth
                helperText="Lowercase letters, numbers, and hyphens only"
              />

              <TextField
                className={classes.field}
                label="Description"
                placeholder="Brief description of the API"
                value={description}
                onChange={e => setDescription(e.target.value)}
                fullWidth
                multiline
                rows={2}
              />

              <Grid container spacing={2}>
                <Grid item xs={6}>
                  <FormControl className={classes.field} fullWidth>
                    <InputLabel>API Type</InputLabel>
                    <Select
                      value={apiType}
                      onChange={e =>
                        setApiType(e.target.value as string)
                      }
                      label="API Type"
                    >
                      {API_TYPES.map(t => (
                        <MenuItem key={t} value={t}>
                          {t.toUpperCase()}
                        </MenuItem>
                      ))}
                    </Select>
                  </FormControl>
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
                label="System"
                value={system}
                onChange={e => setSystem(e.target.value)}
                fullWidth
                helperText="Leave as 'opencode-platform' or specify your own system"
              />

              <Typography variant="subtitle1" className={classes.field}>
                API Specification (OpenAPI JSON)
              </Typography>

              <TextField
                className={classes.field}
                label="OpenAPI Specification"
                placeholder='{"openapi":"3.1.1","info":{"title":"My API","version":"1.0.0"},"paths":{}}'
                value={specDefinition}
                onChange={e => setSpecDefinition(e.target.value)}
                fullWidth
                multiline
                rows={10}
                helperText="Paste your OpenAPI 3.x JSON specification here"
              />

              <div className={classes.actions}>
                <Button
                  variant="contained"
                  color="primary"
                  type="submit"
                  disabled={submitting || !name.trim()}
                >
                  {submitting ? 'Registering...' : 'Register API'}
                </Button>
                <Button
                  variant="outlined"
                  onClick={() => navigate('/')}
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
