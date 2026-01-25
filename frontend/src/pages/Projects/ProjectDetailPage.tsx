import { useEffect, useState } from 'react';
import {
  Container,
  Typography,
  Box,
  CircularProgress,
  Chip,
  Button,
  Alert,
  TextField,
  MenuItem,
  Collapse,
  IconButton,
  InputAdornment,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import AddIcon from '@mui/icons-material/Add';
import SearchIcon from '@mui/icons-material/Search';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
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
  const { entries, loading: entriesLoading, totalCount, pageNumber, pageSize } = useAppSelector(state => state.entries);
  
  const [isFormOpen, setIsFormOpen] = useState(false);
  const [isDeleteDialogOpen, setIsDeleteDialogOpen] = useState(false);
  const [selectedEntry, setSelectedEntry] = useState<Entry | null>(null);
  const [showDetails, setShowDetails] = useState(false);
  const [showFullDescription, setShowFullDescription] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [typeFilter, setTypeFilter] = useState<string>('all');
  const [currentPage, setCurrentPage] = useState(0);
  const [rowsPerPage, setRowsPerPage] = useState(20);

  useEffect(() => {
    iffetchEntriesData();
    }
    return () => {
      dispatch(clearCurrentProject());
    };
  }, [dispatch, id]);

  const fetchEntriesData = () => {
    if (id) {
      dispatch(fetchEntries({ 
        projectId: id, 
        pageNumber: currentPage + 1, 
        pageSize: rowsPerPage 
      }));
    }
  };

  useEffect(() => {
    fetchEntriesData();
  }, [currentPage, rowsPerPagearCurrentProject());
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
    setSfetchEntriesData();

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
      .catfetchEntriesData();
        })
        .catch((err) => {
          console.error('Failed to delete entry:', err);
        });
    }
  };

  const handlePageChange = (newPage: number) => {
    setCurrentPage(newPage);
  };

  const handleRowsPerPageChange = (newRowsPerPage: number) => {
    setRowsPerPage(newRowsPerPage);
    setCurrentPage(0); dispatch(deleteEntry(selectedEntry.id))
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

  const handleClearSearch = () => {
    setSearchQuery('');
    setTypeFilter('all');
    setCurrentPage(0);
    fetchEntriesData();
  };

  const handleSearch = () => {
    setCurrentPage(0);
    fetchEntriesData();
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
    <Container maxWidth="lg" sx={{ py: 4 }}>
      {/* Compact Header */}
      <Box sx={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'flex-start',
        mb: 3,
        pb: 3,
        borderBottom: 1,
        borderColor: 'divider'
      }}>
        <Box sx={{ flex: 1 }}>
          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1 }}>
            <IconButton size="small" onClick={handleBack} sx={{ p: 0.5 }}>
              <ArrowBackIcon />
            </IconButton>
            <Typography variant="h5" component="h1" sx={{ fontWeight: 600, m: 0 }}>
              {currentProject.name}
            </Typography>
          </Box>
          
          {currentProject.description && (
            <Box sx={{ ml: 5 }}>
              <Typography 
                variant="body2" 
                color="text.secondary"
                sx={{
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                  display: '-webkit-box',
                  WebkitLineClamp: showFullDescription ? 'unset' : 1,
                  WebkitBoxOrient: 'vertical',
                  lineHeight: 1.6,
                }}
              >
                {currentProject.description}
              </Typography>
              {currentProject.description.length > 80 && (
                <Button 
                  size="small" 
                  onClick={() => setShowFullDescription(!showFullDescription)}
                  sx={{ p: 0, minWidth: 'auto', mt: 0.5, textTransform: 'none', fontSize: '0.75rem' }}
                >
                  {showFullDescription ? 'Show less' : 'Show more'}
                </Button>
              )}
            </Box>
          )}

          <Box sx={{ ml: 5, mt: 1 }}>
            <Button
              size="small"
              onClick={() => setShowDetails(!showDetails)}
              endIcon={showDetails ? <ExpandLessIcon /> : <ExpandMoreIcon />}
              sx={{ textTransform: 'none', fontSize: '0.75rem', p: 0, minWidth: 'auto' }}
            >
              {showDetails ? 'Hide details' : 'Show details'}
            </Button>
            
            <Collapse in={showDetails}>
              <Box sx={{ mt: 2, display: 'flex', flexDirection: 'column', gap: 1.5 }}>
                <Box>
                  <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 0.5 }}>
                    Created
                  </Typography>
                  <Typography variant="body2">
                    {new Date(currentProject.createdAt).toLocaleString()}
                  </Typography>
                </Box>

                <Box>
                  <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 0.5 }}>
                    Last Updated
                  </Typography>
                  <Typography variant="body2">
                    {new Date(currentProject.updatedAt).toLocaleString()}
                  </Typography>
                </Box>

                {currentProject.tags && currentProject.tags.length > 0 && (
                  <Box>
                    <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mb: 0.5 }}>
                      Tags
                    </Typography>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                      {currentProject.tags.map(tag => (
                        <Chip key={tag} label={tag} size="small" />
                      ))}
                    </Box>
                  </Box>
                )}
              </Box>
            </Collapse>
          </Box>
        </Box>

        <Button 
          variant="outlined" 
          startIcon={<EditIcon />} 
          onClick={handleEdit}
          size="small"
        >
          Edit
        </Button>
      </Box>

      {/* Search and Filter Bar */}
      <Box sx={{ 
        display: 'flex', 
        gap: 2, 
        mb: 3,
        alignItems: 'center',
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
          sx={{ flex: 1 }}
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
          value={typeFilter}
          onChange={(e) => {
            setTypeFilter(e.target.value);
            setCurrentPage(0);
          }}
          size="small"
          sx={{ minWidth: 140 }}
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

        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={handleCreateEntry}
          sx={{ whiteSpace: 'nowrap' }}
        >
          Add Entry
        </Button>
      </Box>

      {/* Entries Table */}
      <EntriesTable
        entries={entries}
        totalCount={totalCount}
        page={currentPage}
        rowsPerPage={rowsPerPage}
        onPageChange={handlePageChange}
        onRowsPerPageChange={handleRowsPerPageChange}
        onEdit={handleEditEntry}
        onDelete={handleDeleteEntry}
        loading={entriesLoading}
      />

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
