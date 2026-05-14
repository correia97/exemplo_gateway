import { useEffect, useState } from 'react';
import {
  Content,
  Header,
  Page,
  Progress,
  ResponseErrorPanel,
  Table,
  TableColumn,
  Link,
} from '@backstage/core-components';
import {
  Button,
  IconButton,
  Tooltip,
  Typography,
  makeStyles,
} from '@material-ui/core';
import EditIcon from '@material-ui/icons/Edit';
import DeleteIcon from '@material-ui/icons/Delete';
import AddIcon from '@material-ui/icons/Add';
import { useApi, identityApiRef } from '@backstage/core-plugin-api';
import {
  catalogApiRef,
  entityRouteRef,
} from '@backstage/plugin-catalog-react';
import {
  type Entity,
  stringifyEntityRef,
} from '@backstage/catalog-model';
import { useNavigate } from 'react-router-dom';
import { useRouteRef } from '@backstage/core-plugin-api';

const useStyles = makeStyles(theme => ({
  actions: {
    marginBottom: theme.spacing(2),
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
}));

export function ApiManagementPage() {
  const classes = useStyles();
  const catalogApi = useApi(catalogApiRef);
  const identityApi = useApi(identityApiRef);
  const navigate = useNavigate();
  const entityLink = useRouteRef(entityRouteRef);

  const [apis, setApis] = useState<Entity[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    async function fetchMyApis() {
      try {
        const identity = await identityApi.getBackstageIdentity();
        const ownerRefs = identity.ownershipEntityRefs ?? [];

        const { items } = await catalogApi.getEntities({
          filter: { kind: 'API' },
          fields: [
            'kind',
            'metadata.name',
            'metadata.namespace',
            'metadata.description',
            'metadata.tags',
            'metadata.uid',
            'spec.owner',
            'spec.type',
            'spec.lifecycle',
          ],
        });

        const myApis = items.filter(entity => {
          const owner = entity.spec?.owner as string | undefined;
          return (
            owner &&
            ownerRefs.some((ref: string) => owner === ref)
          );
        });

        setApis(myApis);
      } catch (err: any) {
        setError(err);
      } finally {
        setLoading(false);
      }
    }

    fetchMyApis();
  }, [catalogApi, identityApi]);

  const handleDelete = async (entity: Entity) => {
    const ref = stringifyEntityRef(entity);
    if (
      window.confirm(`Delete API "${ref}"? This cannot be undone.`)
    ) {
      try {
        await catalogApi.removeEntityByUid(
          entity.metadata.uid!,
        );
        setApis(prev =>
          prev.filter(a => a.metadata.uid !== entity.metadata.uid),
        );
      } catch (err: any) {
        alert(`Failed to delete: ${err.message}`);
      }
    }
  };

  const columns: TableColumn<Entity>[] = [
    {
      title: 'Name',
      field: 'metadata.name',
      render: (row: Entity) => (
        <Link
          to={entityLink({
            kind: 'api',
            namespace: 'default',
            name: row.metadata.name,
          })}
        >
          {row.metadata.name}
        </Link>
      ),
    },
    {
      title: 'Description',
      field: 'metadata.description',
      render: (row: Entity) => row.metadata.description || '-',
    },
    {
      title: 'Type',
      field: 'spec.type',
      width: '100px',
    },
    {
      title: 'Lifecycle',
      field: 'spec.lifecycle',
      width: '120px',
    },
    {
      title: 'Actions',
      width: '120px',
      render: (row: Entity) => (
        <>
          <Tooltip title="Edit API">
            <IconButton
              onClick={() =>
                navigate(
                  `/api-registration/edit/${row.kind}/${row.metadata.namespace || 'default'}/${row.metadata.name}`,
                )
              }
            >
              <EditIcon />
            </IconButton>
          </Tooltip>
          <Tooltip title="Delete API">
            <IconButton onClick={() => handleDelete(row)}>
              <DeleteIcon />
            </IconButton>
          </Tooltip>
        </>
      ),
    },
  ];

  if (loading) return <Progress />;
  if (error) return <ResponseErrorPanel error={error} />;

  return (
    <Page themeId="tool">
      <Header
        title="My APIs"
        subtitle="Manage your registered APIs"
      />
      <Content>
        <div className={classes.actions}>
          <Typography variant="body1">
            {apis.length} API{apis.length !== 1 ? 's' : ''} registered
          </Typography>
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddIcon />}
            onClick={() => navigate('/api-registration')}
          >
            Register New API
          </Button>
        </div>

        {apis.length === 0 ? (
          <Typography variant="body1" color="textSecondary">
            You haven&apos;t registered any APIs yet.{' '}
            <Link to="/api-registration">
              Register your first API
            </Link>
          </Typography>
        ) : (
          <Table
            options={{ paging: false, search: false, toolbar: false }}
            columns={columns}
            data={apis}
          />
        )}
      </Content>
    </Page>
  );
}
