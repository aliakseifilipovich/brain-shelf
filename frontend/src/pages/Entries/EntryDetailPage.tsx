import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Container,
  Typography,
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  IconButton,
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  Edit as EditIcon,
  Link as LinkIcon,
  Description as NoteIcon,
  Code as CodeIcon,
  CheckCircle as TaskIcon,
} from '@mui/icons-material';
import { useAppDispatch, useAppSelector } from '@store/hooks';
import { fetchEntryById, clearCurrentEntry, updateEntry } from '@store/entriesSlice';
import { showSnackbar } from '@store/uiSlice';
import EntryFormDialog from '@components/EntryFormDialog/EntryFormDialog';
import { Entry, EntryType } from '@types/models';

export const EntryDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { currentEntry, loading } = useAppSelector((state) => state.entries);

  const [isFormOpen, setIsFormOpen] = useState(false);

  useEffect(() => {
    if (id) {
      dispatch(fetchEntryById(id));
    }

    return () => {
      dispatch(clearCurrentEntry());
    };
  }, [dispatch, id]);

  const getTypeIcon = () => {
    if (!currentEntry) return null;

    switch (currentEntry.type) {
      case EntryType.Link:
        return <LinkIcon color="primary" fontSize="large" />;
      case EntryType.Note:
        return <NoteIcon color="success" fontSize="large" />;
      case EntryType.Code:
        return <CodeIcon color="warning" fontSize="large" />;
      case EntryType.Task:
        return <TaskIcon color="info" fontSize="large" />;
      default:
        return <NoteIcon fontSize="large" />;
    }
  };

  const getTypeLabel = () => {
    if (!currentEntry) return '';

    switch (currentEntry.type) {
      case EntryType.Link:
        return 'Link';
      case EntryType.Note:
        return 'Note';
      case EntryType.Code:
        return 'Code';
      case EntryType.Task:
        return 'Task';
      default:
        return 'Unknown';
    }
  };

  const getTypeColor = () => {
    if (!currentEntry) return 'default';

    switch (currentEntry.type) {
      case EntryType.Link:
        return 'primary';
      case EntryType.Note:
        return 'success';
      case EntryType.Code:
        return 'warning';
      case EntryType.Task:
        return 'info';
      default:
        return 'default';
    }
  };

  const handleEditClick = () => {
    setIsFormOpen(true);
  };

  const handleFormSubmit = async (entryData: Partial<Entry>) => {
    if (!currentEntry) return;

    try {
      await dispatch(updateEntry({ id: currentEntry.id, entry: entryData })).unwrap();
      dispatch(showSnackbar({ message: 'Entry updated successfully', severity: 'success' }));
      setIsFormOpen(false);
    } catch (error) {
      dispatch(showSnackbar({ message: 'Failed to update entry', severity: 'error' }));
    }
  };

  const handleBackClick = () => {
    navigate('/entries');
  };

  if (loading) {
    return (
      <Container maxWidth="lg">
        <Box display="flex" justifyContent="center" alignItems="center" minHeight="60vh">
          <CircularProgress />
        </Box>
      </Container>
    );
  }

  if (!currentEntry) {
    return (
      <Container maxWidth="lg">
        <Box sx={{ mt: 4, mb: 4 }}>
          <Button startIcon={<ArrowBackIcon />} onClick={handleBackClick} sx={{ mb: 2 }}>
            Back to Entries
          </Button>
          <Typography variant="h5">Entry not found</Typography>
        </Box>
      </Container>
    );
  }

  return (
    <Container maxWidth="lg">
      <Box sx={{ mt: 4, mb: 4 }}>
        <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
          <Button startIcon={<ArrowBackIcon />} onClick={handleBackClick}>
            Back to Entries
          </Button>
          <IconButton color="primary" onClick={handleEditClick}>
            <EditIcon />
          </IconButton>
        </Box>

        <Card>
          <CardContent>
            <Box display="flex" alignItems="center" gap={2} mb={2}>
              {getTypeIcon()}
              <Box>
                <Typography variant="h4" component="h1">
                  {currentEntry.title}
                </Typography>
                <Chip
                  label={getTypeLabel()}
                  color={getTypeColor() as any}
                  size="small"
                  sx={{ mt: 1 }}
                />
              </Box>
            </Box>

            {currentEntry.url && (
              <Box mb={2}>
                <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                  URL
                </Typography>
                <Typography
                  variant="body1"
                  component="a"
                  href={currentEntry.url}
                  target="_blank"
                  rel="noopener noreferrer"
                  sx={{ color: 'primary.main', textDecoration: 'none', '&:hover': { textDecoration: 'underline' } }}
                >
                  {currentEntry.url}
                </Typography>
              </Box>
            )}

            {currentEntry.content && (
              <Box mb={2}>
                <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                  Content
                </Typography>
                <Typography
                  variant="body1"
                  component="pre"
                  sx={{
                    whiteSpace: 'pre-wrap',
                    wordBreak: 'break-word',
                    fontFamily: currentEntry.type === EntryType.Code ? 'monospace' : 'inherit',
                    backgroundColor: currentEntry.type === EntryType.Code ? 'grey.100' : 'transparent',
                    p: currentEntry.type === EntryType.Code ? 2 : 0,
                    borderRadius: currentEntry.type === EntryType.Code ? 1 : 0,
                  }}
                >
                  {currentEntry.content}
                </Typography>
              </Box>
            )}

            {currentEntry.metadata && (
              <Box mb={2}>
                <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                  Metadata
                </Typography>
                <Card variant="outlined" sx={{ p: 2 }}>
                  {currentEntry.metadata.faviconUrl && (
                    <Box display="flex" alignItems="center" gap={1} mb={1}>
                      <img
                        src={currentEntry.metadata.faviconUrl}
                        alt="Favicon"
                        style={{ width: 24, height: 24 }}
                      />
                      <Typography variant="body2">
                        {currentEntry.metadata.siteName || 'Website'}
                      </Typography>
                    </Box>
                  )}
                  {currentEntry.metadata.description && (
                    <Typography variant="body2" color="text.secondary" paragraph>
                      {currentEntry.metadata.description}
                    </Typography>
                  )}
                  {currentEntry.metadata.imageUrl && (
                    <Box mt={1}>
                      <img
                        src={currentEntry.metadata.imageUrl}
                        alt="Preview"
                        style={{ maxWidth: '100%', borderRadius: 4 }}
                      />
                    </Box>
                  )}
                  {currentEntry.metadata.author && (
                    <Typography variant="caption" color="text.secondary">
                      Author: {currentEntry.metadata.author}
                    </Typography>
                  )}
                </Card>
              </Box>
            )}

            {currentEntry.tags && currentEntry.tags.length > 0 && (
              <Box mb={2}>
                <Typography variant="subtitle2" color="text.secondary" gutterBottom>
                  Tags
                </Typography>
                <Box display="flex" gap={1} flexWrap="wrap">
                  {currentEntry.tags.map((tag) => (
                    <Chip key={tag} label={tag} variant="outlined" />
                  ))}
                </Box>
              </Box>
            )}

            <Box
              display="grid"
              gridTemplateColumns={{ xs: '1fr', sm: 'repeat(2, 1fr)' }}
              gap={2}
              mt={3}
              pt={2}
              borderTop={1}
              borderColor="divider"
            >
              <Box>
                <Typography variant="caption" color="text.secondary">
                  Created
                </Typography>
                <Typography variant="body2">
                  {new Date(currentEntry.createdAt).toLocaleString()}
                </Typography>
              </Box>
              <Box>
                <Typography variant="caption" color="text.secondary">
                  Last Updated
                </Typography>
                <Typography variant="body2">
                  {new Date(currentEntry.updatedAt).toLocaleString()}
                </Typography>
              </Box>
            </Box>
          </CardContent>
        </Card>
      </Box>

      <EntryFormDialog
        open={isFormOpen}
        entry={currentEntry}
        onClose={() => setIsFormOpen(false)}
        onSubmit={handleFormSubmit}
      />
    </Container>
  );
};
