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
  TextField,
  MenuItem,
  InputAdornment,
  IconButton,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import AddIcon from '@mui/icons-material/Add';
import SearchIcon from '@mui/icons-material/Search';
import ClearIcon from '@mui/icons-material/Clear';
import { useParams, useNavigate } from 'react-router-dom';
import { useAppDispatch, useAppSelector } from '@store/hooks';
import { fetchProjectById, clearCurrentProject } from '@store/projectsSlice';
import { fetchEntries, createEntry, updateEntry, deleteEntry } from '@store/entriesSlice';
import EntryFormDialog from '@components/EntryFormDialog/EntryFormDialog';
import { Entry, EntryType } from '@types/models';
import { DeleteConfirmDialog } from '@components/DeleteConfirmDialog/DeleteConfirmDialog';
import { EntriesTable } from '@components/EntriesTable/EntriesTable';

export const ProjectDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const dispatch = useAppDispatch();
  const { currentProject, loading, error } = useAppSelector(state => state.projects);
  const { entries, loading: entriesLoading, totalCount } = useAppSelector(state => state.entries);
  
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [selectedEntry, setSelectedEntry] = useState<Entry | null>(null);
  const [searchQuery, setSearchQuery] = useState('');
  const [typeFilter, setTypeFilter] = useState<string>('all');
  const [currentPage, setCurrentPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);

  useEffect(() => {
    if (id) {
      dispatch(fetchProjectById(id));
      fetchEntriesData();
    }
    return () => {
      dispatch(clearCurrentProject());
    };
  }, [dispatch, id]);

  const fetchEntriesData = () => {
    if (!id) return;
    
    const params: any = {
      projectId: id,
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
        fetchEntriesData();
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
          fetchEntriesData();
        })
        .catch((err) => {
          console.error('Failed to delete entry:', err);
        });
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
    setCurrentPage(0);
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

          <Box sx={{ 
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
              <MenuItem value={EntryType.Code}>Code</MenuItem>
              <MenuItem value={EntryType.Task}>Task</MenuItem>
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
            totalCount={totalCount || 0}
            page={currentPage}
            rowsPerPage={rowsPerPage}
            onPageChange={handlePageChange}
            onRowsPerPageChange={handleRowsPerPageChange}
            onEdit={handleEditEntry}
            onDelete={handleDeleteEntry}
            loading={entriesLoading}
          />
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
