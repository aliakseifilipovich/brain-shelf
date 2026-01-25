import { Container, Typography, Box, CircularProgress } from '@mui/material';
import { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '@store/hooks';
import { fetchEntries } from '@store/entriesSlice';

export const EntriesPage = () => {
  const dispatch = useAppDispatch();
  const { loading } = useAppSelector(state => state.entries);

  useEffect(() => {
    dispatch(fetchEntries());
  }, [dispatch]);

  return (
    <Container maxWidth="lg">
      <Box sx={{ mt: 4, mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Entries
        </Typography>
        <Typography variant="body1" color="text.secondary" paragraph>
          Manage your entries here. This page will display all your entries with options to create,
          edit, and delete them.
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
