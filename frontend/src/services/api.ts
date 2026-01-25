import axios, { AxiosInstance, AxiosError } from 'axios';
import { Project, Entry, PaginatedResponse, ApiError, EntryType } from '../types/models';

const API_BASE_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000';

// Create axios instance
const apiClient: AxiosInstance = axios.create({
  baseURL: `${API_BASE_URL}/api`,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor
apiClient.interceptors.request.use(
  config => {
    // Add any authentication tokens here if needed
    return config;
  },
  error => {
    return Promise.reject(error);
  }
);

// Response interceptor
apiClient.interceptors.response.use(
  response => response,
  (error: AxiosError<ApiError>) => {
    // Handle errors globally
    if (error.response) {
      console.error('API Error:', error.response.data);
    } else if (error.request) {
      console.error('Network Error:', error.message);
    } else {
      console.error('Error:', error.message);
    }
    return Promise.reject(error);
  }
);

// Projects API
export const projectsApi = {
  getAll: (params?: { pageNumber?: number; pageSize?: number; searchQuery?: string }) =>
    apiClient.get<PaginatedResponse<Project>>('/projects', { params }),

  getById: (id: string) => apiClient.get<Project>(`/projects/${id}`),

  create: (project: Omit<Project, 'id' | 'createdAt' | 'updatedAt'>) =>
    apiClient.post<Project>('/projects', project),

  update: (id: string, project: Partial<Project>) =>
    apiClient.put<Project>(`/projects/${id}`, project),

  delete: (id: string) => apiClient.delete(`/projects/${id}`),
};

// Entries API
export const entriesApi = {
  getAll: (params?: {
    projectId?: string;
    pageNumber?: number;
    pageSize?: number;
    searchQuery?: string;
    tags?: string[];
  }) =>
    apiClient.get<PaginatedResponse<Entry>>('/entries', {
      params: {
        ...params,
        tags: params?.tags?.join(','),
      },
    }),

  getById: (id: string) => apiClient.get<Entry>(`/entries/${id}`),

  create: (entry: Omit<Entry, 'id' | 'createdAt' | 'updatedAt' | 'metadata'>) =>
    apiClient.post<Entry>('/entries', entry),

  update: (id: string, entry: Partial<Entry>) =>
    apiClient.put<Entry>(`/entries/${id}`, entry),

  delete: (id: string) => apiClient.delete(`/entries/${id}`),

  extractMetadata: (id: string) => apiClient.post<Entry>(`/entries/${id}/extract-metadata`),
};

// Search API
export const searchApi = {
  search: (params: {
    q?: string;
    projectId?: string;
    type?: EntryType;
    fromDate?: string;
    toDate?: string;
    pageNumber?: number;
    pageSize?: number;
  }) => apiClient.get<PaginatedResponse<Entry>>('/search', { params }),
};

export default apiClient;
