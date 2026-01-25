import { useState, useEffect } from 'react';
import {
  Container,
  Typography,
  Box,
  CircularProgress,
  Button,
  TextField,
  InputAdornment,
  Pagination,
  Alert,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import SearchIcon from '@mui/icons-material/Search';
import { useAppDispatch, useAppSelector } from '@store/hooks';
import {
  fetchProjects,
  createProject,
  updateProject,
  deleteProject,
  clearError,
} from '@store/projectsSlice';
import { showSnackbar } from '@store/uiSlice';
import { ProjectCard } from '@components/ProjectCard/ProjectCard';
import { ProjectFormDialog } from '@components/ProjectFormDialog/ProjectFormDialog';
import { DeleteConfirmDialog } from '@components/DeleteConfirmDialog/DeleteConfirmDialog';
import { Project } from '../../types/models';

export const ProjectsPage = () => {
  const dispatch = useAppDispatch();
  const { projects, loading, error, totalPages } = useAppSelector(state => state.projects);

  const [searchQuery, setSearchQuery] = useState('');
  const [currentPage, setCurrentPage] = useState(1);
  const [formDialogOpen, setFormDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [selectedProject, setSelectedProject] = useState<Project | null>(null);

  useEffect(() => {
    dispatch(fetchProjects({ pageNumber: currentPage, searchQuery: searchQuery || undefined }));
  }, [dispatch, currentPage, searchQuery]);

  useEffect(() => {
    if (error) {
      dispatch(showSnackbar({ message: error, severity: 'error' }));
      dispatch(clearError());
    }
  }, [error, dispatch]);

  const handleSearch = (value: string) => {
    setSearchQuery(value);
    setCurrentPage(1);
  };

  const handlePageChange = (_event: React.ChangeEvent<unknown>, value: number) => {
    setCurrentPage(value);
  };

  const handleCreateClick = () => {
    setSelectedProject(null);
    setFormDialogOpen(true);
  };

  const handleEditClick = (project: Project) => {
    setSelectedProject(project);
    setFormDialogOpen(true);
  };

  const handleDeleteClick = (project: Project) => {
    setSelectedProject(project);
    setDeleteDialogOpen(true);
  };

  const handleFormSubmit = async (data: Omit<Project, 'id' | 'createdAt' | 'updatedAt'>) => {
    try {
      if (selectedProject) {
        await dispatch(updateProject({ id: selectedProject.id, data })).unwrap();
        dispatch(showSnackbar({ message: 'Project updated successfully', severity: 'success' }));
      } else {
        await dispatch(createProject(data)).unwrap();
        dispatch(showSnackbar({ message: 'Project created successfully', severity: 'success' }));
      }
      dispatch(fetchProjects({ pageNumber: currentPage, searchQuery: searchQuery || undefined }));
    } catch (err) {
      dispatch(
        showSnackbar({
          message: `Failed to ${selectedProject ? 'update' : 'create'} project`,
          severity: 'error',
        })
      );
    }
  };

  const handleDeleteConfirm = async () => {
    if (selectedProject) {
      try {
        await dispatch(deleteProject(selectedProject.id)).unwrap();
        dispatch(showSnackbar({ message: 'Project deleted successfully', severity: 'success' }));
        setDeleteDialogOpen(false);
        dispatch(fetchProjects({ pageNumber: currentPage, searchQuery: searchQuery || undefined }));
      } catch (err) {
        dispatch(showSnackbar({ message: 'Failed to delete project', severity: 'error' }));
      }
    }
  };

  return (
    <Container maxWidth="lg">
      <Box sx={{ mt: 4, mb: 4 }}>
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
          <Typography variant="h4" component="h1">
            Projects
          </Typography>
          <Button
            variant="contained"
            color="primary"
            startIcon={<AddIcon />}
            onClick={handleCreateClick}
          >
            Create Project
          </Button>
        </Box>

        <TextField
          fullWidth
          placeholder="Search projects..."
          value={searchQuery}
          onChange={e => handleSearch(e.target.value)}
          InputProps={{
            startAdornment: (
              <InputAdornment position="start">
                <SearchIcon />
              </InputAdornment>
            ),
          }}
          sx={{ mb: 3 }}
        />

        {loading && (
          <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
            <CircularProgress />
          </Box>
        )}

        {!loading && projects.length === 0 && (
          <Alert severity="info">
            {searchQuery
              ? 'No projects found matching your search.'
              : 'No projects yet. Create your first project to get started!'}
          </Alert>
        )}

        {!loading && projects.length > 0 && (
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
              }}
            >
              {projects.map(project => (
                <ProjectCard
                  key={project.id}
                  project={project}
                  onEdit={handleEditClick}
                  onDelete={handleDeleteClick}
                />
              ))}
            </Box>

            {totalPages > 1 && (
              <Box sx={{ display: 'flex', justifyContent: 'center', mt: 4 }}>
                <Pagination
                  count={totalPages}
                  page={currentPage}
                  onChange={handlePageChange}
                  color="primary"
                />
              </Box>
            )}
          </>
        )}
      </Box>

      <ProjectFormDialog
        open={formDialogOpen}
        project={selectedProject}
        onClose={() => setFormDialogOpen(false)}
        onSubmit={handleFormSubmit}
      />

      <DeleteConfirmDialog
        open={deleteDialogOpen}
        title="Delete Project"
        message={`Are you sure you want to delete "${selectedProject?.name}"? This action cannot be undone.`}
        onConfirm={handleDeleteConfirm}
        onCancel={() => setDeleteDialogOpen(false)}
      />
    </Container>
  );
};
