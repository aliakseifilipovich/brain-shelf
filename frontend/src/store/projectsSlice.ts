import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { Project, PaginatedResponse } from '../types/models';
import { projectsApi } from '@services/api';

interface ProjectsState {
  projects: Project[];
  currentProject: Project | null;
  loading: boolean;
  error: string | null;
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

const initialState: ProjectsState = {
  projects: [],
  currentProject: null,
  loading: false,
  error: null,
  totalCount: 0,
  pageNumber: 1,
  pageSize: 10,
  totalPages: 0,
};

export const fetchProjects = createAsyncThunk(
  'projects/fetchProjects',
  async (params: { pageNumber?: number; pageSize?: number; searchQuery?: string } = {}) => {
    const response = await projectsApi.getAll(params);
    return response.data;
  }
);

export const fetchProjectById = createAsyncThunk(
  'projects/fetchProjectById',
  async (id: number) => {
    const response = await projectsApi.getById(id);
    return response.data;
  }
);

export const createProject = createAsyncThunk(
  'projects/createProject',
  async (project: Omit<Project, 'id' | 'createdAt' | 'updatedAt'>) => {
    const response = await projectsApi.create(project);
    return response.data;
  }
);

export const updateProject = createAsyncThunk(
  'projects/updateProject',
  async ({ id, data }: { id: number; data: Partial<Project> }) => {
    const response = await projectsApi.update(id, data);
    return response.data;
  }
);

export const deleteProject = createAsyncThunk(
  'projects/deleteProject',
  async (id: number) => {
    await projectsApi.delete(id);
    return id;
  }
);

const projectsSlice = createSlice({
  name: 'projects',
  initialState,
  reducers: {
    clearError: state => {
      state.error = null;
    },
    clearCurrentProject: state => {
      state.currentProject = null;
    },
  },
  extraReducers: builder => {
    builder
      // Fetch projects
      .addCase(fetchProjects.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(
        fetchProjects.fulfilled,
        (state, action: PayloadAction<PaginatedResponse<Project>>) => {
          state.loading = false;
          state.projects = action.payload.items;
          state.totalCount = action.payload.totalCount;
          state.pageNumber = action.payload.pageNumber;
          state.pageSize = action.payload.pageSize;
          state.totalPages = action.payload.totalPages;
        }
      )
      .addCase(fetchProjects.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to fetch projects';
      })
      // Fetch project by ID
      .addCase(fetchProjectById.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchProjectById.fulfilled, (state, action: PayloadAction<Project>) => {
        state.loading = false;
        state.currentProject = action.payload;
      })
      .addCase(fetchProjectById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to fetch project';
      })
      // Create project
      .addCase(createProject.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createProject.fulfilled, (state, action: PayloadAction<Project>) => {
        state.loading = false;
        state.projects.push(action.payload);
        state.totalCount += 1;
      })
      .addCase(createProject.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to create project';
      })
      // Update project
      .addCase(updateProject.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateProject.fulfilled, (state, action: PayloadAction<Project>) => {
        state.loading = false;
        const index = state.projects.findIndex(p => p.id === action.payload.id);
        if (index !== -1) {
          state.projects[index] = action.payload;
        }
        if (state.currentProject?.id === action.payload.id) {
          state.currentProject = action.payload;
        }
      })
      .addCase(updateProject.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to update project';
      })
      // Delete project
      .addCase(deleteProject.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteProject.fulfilled, (state, action: PayloadAction<number>) => {
        state.loading = false;
        state.projects = state.projects.filter(p => p.id !== action.payload);
        state.totalCount -= 1;
        if (state.currentProject?.id === action.payload) {
          state.currentProject = null;
        }
      })
      .addCase(deleteProject.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to delete project';
      });
  },
});

export const { clearError, clearCurrentProject } = projectsSlice.actions;
export default projectsSlice.reducer;
