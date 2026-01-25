import { useEffect } from 'react';
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
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import { useParams, useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@store/hooks';
import { fetchProjectById, clearCurrentProject } from '@store/projectsSlice';

export const ProjectDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { currentProject, loading, error } = useAppSelector(state => state.projects);

  useEffect(() => {
    if (id) {
      dispatch(fetchProjectById(Number(id)));
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
          <Typography variant="h5" gutterBottom>
            Entries
          </Typography>
          <Alert severity="info">
            Entry list will be implemented in the next issue. For now, navigate to the Entries page
            to manage entries for this project.
          </Alert>
        </Box>
      </Box>
    </Container>
  );
};
