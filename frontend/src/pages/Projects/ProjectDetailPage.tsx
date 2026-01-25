import { Container, Typography, Box, CircularProgress } from '@mui/material';
import { useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@store/hooks';
import { fetchProjectById } from '@store/projectsSlice';

export const ProjectDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const dispatch = useAppDispatch();
  const { currentProject, loading } = useAppSelector(state => state.projects);

  useEffect(() => {
    if (id) {
      dispatch(fetchProjectById(Number(id)));
    }
  }, [dispatch, id]);

  return (
    <Container maxWidth="lg">
      <Box sx={{ mt: 4, mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          {currentProject?.name || 'Project Details'}
        </Typography>
        <Typography variant="body1" color="text.secondary" paragraph>
          {currentProject?.description ||
            'This page will display detailed information about the selected project.'}
        </Typography>
        {loading && (
          <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
            <CircularProgress />
          </Box>
        )}
      </Box>
    </Container>
  );
};
