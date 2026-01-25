import React, { useEffect, useState } from 'react';
import {
  Container,
  Typography,
  Box,
  Button,
  TextField,
  MenuItem,
  CircularProgress,
  InputAdornment,
  IconButton,
} from '@mui/material';
import { Add as AddIcon } from '@mui/icons-material';
import SearchIcon from '@mui/icons-material/Search';
import ClearIcon from '@mui/icons-material/Clear';
import { useAppDispatch, useAppSelector } from '@store/hooks';
import {
  fetchEntries,
  createEntry,
  updateEntry,
  deleteEntry,
} from '@store/entriesSlice';
import { showSnackbar } from '@store/uiSlice';
import { EntriesTable } from '@components/EntriesTable/EntriesTable';
import EntryFormDialog from '@components/EntryFormDialog/EntryFormDialog';
import { DeleteConfirmDialog } from '@components/DeleteConfirmDialog/DeleteConfirmDialog';
import { Entry, EntryType } from '@types/models';

export const EntriesPage = () => {
  const dispatch = useAppDispatch();
  const { entries, loading, totalCount, pageNumber, pageSize } = useAppSelector((state) => state.entries);

  const [searchQuery, setSearchQuery] = useState('');
  const [typeFilter, setTypeFilter] = useState<string>('all');
  const [currentPage, setCurrentPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [selectedEntry, setSelectedEntry] = useState<Entry | null>(null);

  const fetchEntriesData = () => {
    const params: any = {
      pageNumber: currentPage + 1,
      pageSize: rowsPerPage,
    };

    if (searchQuery) {
      params.searchQuery = searchQuery;
    }

    if (typeFilter !== 'all') {
      params.type = Number(typeFilter);
    }

    dispatch(fetchEntries(params));
  };

  useEffect(() => {
    fetchEntriesData();
  }, [currentPage, rowsPerPage]);

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
        await dispatch(updateEntry({ id: selectedEntry.id, data: entryData })).unwrap();
        dispatch(showSnackbar({ message: 'Entry updated successfully', severity: 'success' }));
      } else {
        await dispatch(createEntry(entryData)).unwrap();
        dispatch(showSnackbar({ message: 'Entry created successfully', severity: 'success' }));
      }
      setIsFormOpen(false);
      fetchEntriesData();
    } catch (error) {
      dispatch(
        showSnackbar({
          message: `Failed to ${selectedEntry ? 'update' : 'create'} entry`,
          severity: 'error',
        })
      );
    }
  };
  };

  const handleDeleteConfirm = async () => {
    if (!selectedEntry) return;

    try {
      await dispatch(deleteEntry(selectedEntry.id)).unwrap();
      dispatch(showSnackbar({ message: 'Entry deleted successfully', severity: 'success' }));
      setIsDeleteDialogOpen(false);
      // Refresh the entries list
      const params: any = {
        pageNumber: page,
        pageSize: 20,
      };
      if (searchQuery) {
        params.searchQuery = searchQuery;
      }
      if (typeFilter !== 'all') {
        params.type = Number(typeFilter);
      }
      fetchEntriesData();
    } catch (error) {
      dispatch(showSnackbar({ message: 'Failed to delete entry', severity: 'error' }));
    }
  };

  const handleSearch = () => {
    setCurrentPage(0);
    fetchEntriesData();
  };

  const handleClearSearch = () => {
    setSearchQuery('');
    setTypeFilter('all');
    setCurrentPage(0);
    fetchEntriesData();
  };

  const handlePageChange = (newPage: number) => {
    setCurrentPage(newPage);
  };

  const handleRowsPerPageChange = (newRowsPerPage: number) => {
    setRowsPerPage(newRowsPerPage);
    setCurrentPage(0
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
            <sx={{ 
          display: 'flex', 
          gap: 2, 
          mb: 3,
          backgroundColor: 'background.paper',
          p: 2,
          borderRadius: 1,
          border: 1,
          borderColor: 'divider'
        }}>
          <TextField
            placeholder="Search entries..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            onKeyPress={(e) => e.key === 'Enter' && handleSearch()}
            size="small"
            sx={{ flexGrow: 1 }}
            InputProps={{
              startAdornment: (
                <InputAdornment position="start">
                  <SearchIcon fontSize="small" />
                </InputAdornment>
              ),
              endAdornment: searchQuery && (
                <InputAdornment position="end">
                  <IconButton size="small" onClick={handleClearSearch}>
                    <ClearIcon fontSize="small" />
                  </IconButton>
                </InputAdornment>
              ),
            }}
          />
          <TextField
            select
            label="Type"
            value={typeFilter}
            onChange={(e) => {
              setTypeFilter(e.target.value);
              setCurrentPage(0);
            }}
            size="small"
            sx={{ minWidth: 150 }}
          >
            <MenuItem value="all">All Types</MenuItem>
            <MenuItem value={EntryType.Link}>Link</MenuItem>
            <MenuItem value={EntryType.Note}>Note</MenuItem>
            <MenuItem value={EntryType.Setting}>Setting</MenuItem>
            <MenuItem value={EntryType.Instruction}>Instruction</MenuItem>
          </TextField>
          <Button
            variant="outlined"
            onClick={handleSearch}
            sx={{ whiteSpace: 'nowrap' }}
          >
            Search
          </Button>
        </Box>

        <EntriesTable
          entries={entries}
          totalCount={totalCount}
          page={currentPage}
          rowsPerPage={rowsPerPage}
          onPageChange={handlePageChange}
          onRowsPerPageChange={handleRowsPerPageChange}
          onEdit={handleEditClick}
          onDelete={handleDeleteClick}
          loading={loading}
        />
    </Container>
  );
};
