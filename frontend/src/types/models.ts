export interface Project {
  id: number;
  name: string;
  description?: string;
  tags?: string[];
  createdAt: string;
  updatedAt: string;
}

export interface Entry {
  id: number;
  projectId: number;
  projectName?: string;
  title: string;
  description?: string;
  content?: string;
  url?: string;
  type: EntryType;
  tags?: string[];
  tagNames?: string[];
  metadataId?: number;
  metadata?: Metadata;
  createdAt: string;
  updatedAt: string;
}

export enum EntryType {
  Note = 0,
  Link = 1,
  Code = 2,
  Task = 3,
}

export interface Metadata {
  id: number;
  title?: string;
  description?: string;
  keywords?: string;
  imageUrl?: string;
  faviconUrl?: string;
  author?: string;
  siteName?: string;
  createdAt: string;
  updatedAt: string;
}

export interface Tag {
  id: number;
  name: string;
  createdAt: string;
  updatedAt: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface ApiError {
  type: string;
  title: string;
  status: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
