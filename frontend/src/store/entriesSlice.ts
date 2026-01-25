import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import { Entry, PaginatedResponse } from '../types/models';
import { entriesApi } from '@services/api';

interface EntriesState {
  entries: Entry[];
  currentEntry: Entry | null;
  loading: boolean;
  error: string | null;
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

const initialState: EntriesState = {
  entries: [],
  currentEntry: null,
  loading: false,
  error: null,
  totalCount: 0,
  pageNumber: 1,
  pageSize: 10,
  totalPages: 0,
};

export const fetchEntries = createAsyncThunk(
  'entries/fetchEntries',
  async (params: {
    projectId?: string;
    pageNumber?: number;
    pageSize?: number;
    searchQuery?: string;
    tags?: string[];
  } = {}) => {
    const response = await entriesApi.getAll(params);
    return response.data;
  }
);

export const fetchEntryById = createAsyncThunk(
  'entries/fetchEntryById',
  async (id: string) => {
    const response = await entriesApi.getById(id);
    return response.data;
  }
);

export const createEntry = createAsyncThunk(
  'entries/createEntry',
  async (entry: Omit<Entry, 'id' | 'createdAt' | 'updatedAt' | 'metadata'>) => {
    const response = await entriesApi.create(entry);
    return response.data;
  }
);

export const updateEntry = createAsyncThunk(
  'entries/updateEntry',
  async ({ id, data }: { id: string; data: Partial<Entry> }) => {
    const response = await entriesApi.update(id, data);
    return response.data;
  }
);

export const deleteEntry = createAsyncThunk('entries/deleteEntry', async (id: string) => {
  await entriesApi.delete(id);
  return id;
});

export const extractMetadata = createAsyncThunk(
  'entries/extractMetadata',
  async (id: string) => {
    const response = await entriesApi.extractMetadata(id);
    return response.data;
  }
);

const entriesSlice = createSlice({
  name: 'entries',
  initialState,
  reducers: {
    clearError: state => {
      state.error = null;
    },
    clearCurrentEntry: state => {
      state.currentEntry = null;
    },
  },
  extraReducers: builder => {
    builder
      // Fetch entries
      .addCase(fetchEntries.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(
        fetchEntries.fulfilled,
        (state, action: PayloadAction<PaginatedResponse<Entry>>) => {
          state.loading = false;
          state.entries = action.payload.items;
          state.totalCount = action.payload.totalCount;
          state.pageNumber = action.payload.pageNumber;
          state.pageSize = action.payload.pageSize;
          state.totalPages = action.payload.totalPages;
        }
      )
      .addCase(fetchEntries.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to fetch entries';
      })
      // Fetch entry by ID
      .addCase(fetchEntryById.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchEntryById.fulfilled, (state, action: PayloadAction<Entry>) => {
        state.loading = false;
        state.currentEntry = action.payload;
      })
      .addCase(fetchEntryById.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to fetch entry';
      })
      // Create entry
      .addCase(createEntry.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(createEntry.fulfilled, (state, action: PayloadAction<Entry>) => {
        state.loading = false;
        state.entries.push(action.payload);
        state.totalCount += 1;
      })
      .addCase(createEntry.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to create entry';
      })
      // Update entry
      .addCase(updateEntry.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(updateEntry.fulfilled, (state, action: PayloadAction<Entry>) => {
        state.loading = false;
        const index = state.entries.findIndex(e => e.id === action.payload.id);
        if (index !== -1) {
          state.entries[index] = action.payload;
        }
        if (state.currentEntry?.id === action.payload.id) {
          state.currentEntry = action.payload;
        }
      })
      .addCase(updateEntry.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to update entry';
      })
      // Delete entry
      .addCase(deleteEntry.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(deleteEntry.fulfilled, (state, action: PayloadAction<number>) => {
        state.loading = false;
        state.entries = state.entries.filter(e => e.id !== action.payload);
        state.totalCount -= 1;
        if (state.currentEntry?.id === action.payload) {
          state.currentEntry = null;
        }
      })
      .addCase(deleteEntry.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to delete entry';
      })
      // Extract metadata
      .addCase(extractMetadata.pending, state => {
        state.loading = true;
        state.error = null;
      })
      .addCase(extractMetadata.fulfilled, (state, action: PayloadAction<Entry>) => {
        state.loading = false;
        const index = state.entries.findIndex(e => e.id === action.payload.id);
        if (index !== -1) {
          state.entries[index] = action.payload;
        }
        if (state.currentEntry?.id === action.payload.id) {
          state.currentEntry = action.payload;
        }
      })
      .addCase(extractMetadata.rejected, (state, action) => {
        state.loading = false;
        state.error = action.error.message || 'Failed to extract metadata';
      });
  },
});

export const { clearError, clearCurrentEntry } = entriesSlice.actions;
export default entriesSlice.reducer;
