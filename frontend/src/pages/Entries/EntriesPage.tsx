import React, { useEffect, useState } from 'react';
import {
  Container,
  Typography,
  Box,
  Button,
  TextField,
  MenuItem,
  Pagination,
  CircularProgress,
} from '@mui/material';
import { Add as AddIcon } from '@mui/icons-material';
import { useAppDispatch, useAppSelector } from '@store/hooks';
import {
  fetchEntries,
  createEntry,
  updateEntry,
  deleteEntry,
} from '@store/entriesSlice';
import { showSnackbar } from '@store/uiSlice';
import EntryCard from '@components/EntryCard/EntryCard';
import EntryFormDialog from '@components/EntryFormDialog/EntryFormDialog';
import DeleteConfirmDialog from '@components/DeleteConfirmDialog/DeleteConfirmDialog';
import { Entry, EntryType } from '@types/models';

export const EntriesPage = () => {
  const dispatch = useAppDispatch();
  const { items: entries, loading, totalPages } = useAppSelector((state) => state.entries);

  const [searchQuery, setSearchQuery] = useState('');
  const [typeFilter, setTypeFilter] = useState<string>('all');
  const [page, setPage] = useState(1);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [selectedEntry, setSelectedEntry] = useState<Entry | null>(null);

  useEffect(() => {
    const params: any = {
      pageNumber: page,
      pageSize: 20,
    };

    if (searchQuery) {
      params.searchTerm = searchQuery;
    }

    if (typeFilter !== 'all') {
      params.type = Number(typeFilter);
    }

    dispatch(fetchEntries(params));
  }, [dispatch, page, searchQuery, typeFilter]);

  const handleCreateClick = () => {
    setSelectedEntry(null);
    setIsFormOpen(true);
  };

  const handleEditClick = (entry: Entry) => {
    setSelectedEntry(entry);
    setIsFormOpen(true);
  };

  const handleDeleteClick = (entry: Entry) => {
    setSelectedEntry(entry);
    setIsDeleteDialogOpen(true);
  };

  const handleFormSubmit = async (entryData: Partial<Entry>) => {
    try {
      if (selectedEntry) {
        await dispatch(updateEntry({ id: selectedEntry.id, entry: entryData })).unwrap();
        dispatch(showSnackbar({ message: 'Entry updated successfully', severity: 'success' }));
      } else {
        await dispatch(createEntry(entryData)).unwrap();
        dispatch(showSnackbar({ message: 'Entry created successfully', severity: 'success' }));
      }
      setIsFormOpen(false);
    } catch (error) {
      dispatch(
        showSnackbar({
          message: `Failed to ${selectedEntry ? 'update' : 'create'} entry`,
          severity: 'error',
        })
      );
    }
  };

  const handleDeleteConfirm = async () => {
    if (!selectedEntry) return;

    try {
      await dispatch(deleteEntry(selectedEntry.id)).unwrap();
      dispatch(showSnackbar({ message: 'Entry deleted successfully', severity: 'success' }));
      setIsDeleteDialogOpen(false);
    } catch (error) {
      dispatch(showSnackbar({ message: 'Failed to delete entry', severity: 'error' }));
    }
  };

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setSearchQuery(e.target.value);
    setPage(1);
  };

  const handleTypeFilterChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setTypeFilter(e.target.value);
    setPage(1);
  };

  const handlePageChange = (_event: React.ChangeEvent<unknown>, value: number) => {
    setPage(value);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  return (
    <Container maxWidth="lg">
      <Box sx={{ mt: 4, mb: 4 }}>
        <Box display="flex" justifyContent="space-between" alignItems="center" mb={3}>
          <Typography variant="h4" component="h1">
            Entries
          </Typography>
          <Button variant="contained" startIcon={<AddIcon />} onClick={handleCreateClick}>
            New Entry
          </Button>
        </Box>

        <Box display="flex" gap={2} mb={3}>
          <TextField
            placeholder="Search entries..."
            value={searchQuery}
            onChange={handleSearchChange}
            size="small"
            sx={{ flexGrow: 1 }}
          />
          <TextField
            select
            label="Type"
            value={typeFilter}
            onChange={handleTypeFilterChange}
            size="small"
            sx={{ minWidth: 150 }}
          >
            <MenuItem value="all">All Types</MenuItem>
            <MenuItem value={EntryType.Note}>Note</MenuItem>
            <MenuItem value={EntryType.Link}>Link</MenuItem>
            <MenuItem value={EntryType.Code}>Code</MenuItem>
            <MenuItem value={EntryType.Task}>Task</MenuItem>
          </TextField>
        </Box>

        {loading && (
          <Box display="flex" justifyContent="center" py={4}>
            <CircularProgress />
          </Box>
        )}

        {!loading && entries.length === 0 && (
          <Box textAlign="center" py={4}>
            <Typography variant="body1" color="text.secondary">
              No entries found. Create your first entry to get started!
            </Typography>
          </Box>
        )}

        {!loading && entries.length > 0 && (
          <>
            <Box
              sx={{
                display: 'grid',
                gridTemplateColumns: {
                  xs: '1fr',
                  sm: 'repeat(2, 1fr)',
                  md: 'repeat(3, 1fr)',
                },
                gap: 3,
                mb: 4,
              }}
            >
              {entries.map((entry) => (
                <EntryCard
                  key={entry.id}
                  entry={entry}
                  onEdit={handleEditClick}
                  onDelete={handleDeleteClick}
                />
              ))}
            </Box>

            {totalPages > 1 && (
              <Box display="flex" justifyContent="center" mt={4}>
                <Pagination
                  count={totalPages}
                  page={page}
                  onChange={handlePageChange}
                  color="primary"
                />
              </Box>
            )}
          </>
        )}
      </Box>

      <EntryFormDialog
        open={isFormOpen}
        entry={selectedEntry}
        onClose={() => setIsFormOpen(false)}
        onSubmit={handleFormSubmit}
      />

      <DeleteConfirmDialog
        open={isDeleteDialogOpen}
        title="Delete Entry"
        message={`Are you sure you want to delete "${selectedEntry?.title}"? This action cannot be undone.`}
        onClose={() => setIsDeleteDialogOpen(false)}
        onConfirm={handleDeleteConfirm}
      />
    </Container>
  );
};
