import { Container, Typography, Box, CircularProgress } from '@mui/material';
import { useEffect } from 'react';
import { useParams } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@store/hooks';
import { fetchEntryById } from '@store/entriesSlice';

export const EntryDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const dispatch = useAppDispatch();
  const { currentEntry, loading } = useAppSelector(state => state.entries);

  useEffect(() => {
    if (id) {
      dispatch(fetchEntryById(Number(id)));
    }
  }, [dispatch, id]);

  return (
    <Container maxWidth="lg">
      <Box sx={{ mt: 4, mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          {currentEntry?.title || 'Entry Details'}
        </Typography>
        <Typography variant="body1" color="text.secondary" paragraph>
          {currentEntry?.content ||
            'This page will display detailed information about the selected entry.'}
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
