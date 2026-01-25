import { useEffect, useState } from 'react';
import {
  Container,
  Typography,
  Box,
  CircularProgress,
  Card,
  CardContent,
  Chip,
  Button,
  Alert,
  Divider,
  Grid,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import AddIcon from '@mui/icons-material/Add';
import { useParams, useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@store/hooks';
import { fetchProjectById, clearCurrentProject } from '@store/projectsSlice';
import { fetchEntries, createEntry, updateEntry, deleteEntry } from '@store/entriesSlice';
import EntryCard from '@components/EntryCard/EntryCard';
import EntryFormDialog from '@components/EntryFormDialog/EntryFormDialog';
import { Entry } from '@types/models';
import { DeleteConfirmDialog } from '@components/DeleteConfirmDialog/DeleteConfirmDialog';

export const ProjectDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { currentProject, loading, error } = useAppSelector(state => state.projects);
  const { entries, loading: entriesLoading } = useAppSelector(state => state.entries);
  
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [selectedEntry, setSelectedEntry] = useState<Entry | null>(null);

  useEffect(() => {
    if (id) {
      dispatch(fetchProjectById(id));
      dispatch(fetchEntries({ projectId: id, pageSize: 100 }));
    }
    return () => {
      dispatch(clearCurrentProject());
    };
  }, [dispatch, id]);

  const handleBack = () => {
    navigate('/projects');
  };

  const handleEdit = () => {
    navigate('/projects');
  };

  const handleCreateEntry = () => {
    setSelectedEntry(null);
    setIsFormOpen(true);
  };

  const handleEditEntry = (entry: Entry) => {
    setSelectedEntry(entry);
    setIsFormOpen(true);
  };

  const handleDeleteEntry = (entry: Entry) => {
    setSelectedEntry(entry);
    setIsDeleteDialogOpen(true);
  };

  const handleFormSubmit = (data: any) => {
    const action = selectedEntry
      ? dispatch(updateEntry({ id: selectedEntry.id, data }))
      : dispatch(createEntry(data));

    action
      .unwrap()
      .then(() => {
        setIsFormOpen(false);
        setSelectedEntry(null);
        if (id) {
          dispatch(fetchEntries({ projectId: id, pageSize: 100 }));
        }
      })
      .catch((err) => {
        console.error('Failed to save entry:', err);
      });
  };

  const handleDeleteConfirm = () => {
    if (selectedEntry) {
      dispatch(deleteEntry(selectedEntry.id))
        .unwrap()
        .then(() => {
          setIsDeleteDialogOpen(false);
          setSelectedEntry(null);
          if (id) {
            dispatch(fetchEntries({ projectId: id, pageSize: 100 }));
          }
        })
        .catch((err) => {
          console.error('Failed to delete entry:', err);
        });
    }
  };

  if (loading) {
    return (
      <Container maxWidth="lg">
        <Box sx={{ display: 'flex', justifyContent: 'center', mt: 8 }}>
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (error) {
    return (
      <Container maxWidth="lg">
        <Box sx={{ mt: 4 }}>
          <Alert severity="error">{error}</Alert>
          <Button startIcon={<ArrowBackIcon />} onClick={handleBack} sx={{ mt: 2 }}>
            Back to Projects
          </Button>
        </Box>
      </Container>
    );
  }

  if (!currentProject) {
    return (
      <Container maxWidth="lg">
        <Box sx={{ mt: 4 }}>
          <Alert severity="warning">Project not found</Alert>
          <Button startIcon={<ArrowBackIcon />} onClick={handleBack} sx={{ mt: 2 }}>
            Back to Projects
          </Button>
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg">
      <Box sx={{ mt: 4, mb: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Button startIcon={<ArrowBackIcon />} onClick={handleBack}>
            Back to Projects
          </Button>
          <Button variant="contained" startIcon={<EditIcon />} onClick={handleEdit}>
            Edit Project
          </Button>
        </Box>

        <Card>
          <CardContent>
            <Typography variant="h4" component="h1" gutterBottom>
              {currentProject.name}
            </Typography>

            {currentProject.description && (
              <Typography variant="body1" color="text.secondary" paragraph sx={{ mt: 2 }}>
                {currentProject.description}
              </Typography>
            )}

            <Divider sx={{ my: 3 }} />

            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: { xs: '1fr', sm: 'repeat(2, 1fr)' },
                gap: 2,
              }}
            >
              <Box>
                <Typography variant="subtitle2" color="text.secondary">
                  Created
                </Typography>
                <Typography variant="body1">
                  {new Date(currentProject.createdAt).toLocaleString()}
                </Typography>
              </Box>

              <Box>
                <Typography variant="subtitle2" color="text.secondary">
                  Last Updated
                </Typography>
                <Typography variant="body1">
                  {new Date(currentProject.updatedAt).toLocaleString()}
                </Typography>
              </Box>

              {currentProject.tags && currentProject.tags.length > 0 && (
                <Box sx={{ gridColumn: '1 / -1' }}>
                  <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                    Tags
                  </Typography>
                  <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 1 }}>
                    {currentProject.tags.map(tag => (
                      <Chip key={tag} label={tag} />
                    ))}
                  </Box>
                </Box>
              )}
            </Box>
          </CardContent>
        </Card>

        <Box sx={{ mt: 4 }}>
          <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
            <Typography variant="h5">
              Entries
            </Typography>
            <Button variant="contained" startIcon={<AddIcon />} onClick={handleCreateEntry}>
              Add Entry
            </Button>
          </Box>

          {entriesLoading ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
              <CircularProgress />
            </Box>
          ) : entries.length === 0 ? (
            <Alert severity="info">
              No entries found for this project. Click "Add Entry" to create one.
            </Alert>
          ) : (
            <Grid container spacing={3}>
              {entries.map(entry => (
                <Grid item xs={12} sm={6} md={4} key={entry.id}>
                  <EntryCard
                    entry={entry}
                    onEdit={handleEditEntry}
                    onDelete={handleDeleteEntry}
                  />
                </Grid>
              ))}
            </Grid>
          )}
        </Box>
      </Box>

      <EntryFormDialog
        open={isFormOpen}
        entry={selectedEntry}
        projectId={currentProject.id}
        onClose={() => {
          setIsFormOpen(false);
          setSelectedEntry(null);
        }}
        onSubmit={handleFormSubmit}
      />

      <DeleteConfirmDialog
        open={isDeleteDialogOpen}
        title="Delete Entry"
        message={`Are you sure you want to delete "${selectedEntry?.title}"?`}
        onClose={() => {
          setIsDeleteDialogOpen(false);
          setSelectedEntry(null);
        }}
        onConfirm={handleDeleteConfirm}
      />
    </Container>
  );
};
