import React, { useState, useEffect, useCallback, useRef } from 'react';
import {
  Box,
  Container,
  TextField,
  InputAdornment,
  Paper,
  Typography,
  Chip,
  MenuItem,
  Select,
  FormControl,
  InputLabel,
  Autocomplete,
  Card,
  CardContent,
  CardActions,
  Button,
  IconButton,
  Collapse,
  Alert,
  CircularProgress,
  Stack,
  Pagination,
  SelectChangeEvent,
} from '@mui/material';
import {
  Search as SearchIcon,
  FilterList as FilterIcon,
  Clear as ClearIcon,
  OpenInNew as OpenInNewIcon,
  CalendarToday as CalendarIcon,
} from '@mui/icons-material';
import { useNavigate } from 'react-router-dom';
import { searchApi } from '../../services/api';
import { Entry, EntryType, PaginatedResponse } from '../../types/models';
import { useAppSelector } from '../../store/hooks';

interface SearchFilters {
  projectId?: number;
  type?: EntryType;
  fromDate?: string;
  toDate?: string;
}

const ENTRY_TYPE_LABELS: Record<EntryType, string> = {
  [EntryType.Note]: 'Note',
  [EntryType.Link]: 'Link',
  [EntryType.Code]: 'Code',
  [EntryType.Task]: 'Task',
};

const ENTRY_TYPE_COLORS: Record<EntryType, 'default' | 'primary' | 'secondary' | 'success' | 'error' | 'info' | 'warning'> = {
  [EntryType.Note]: 'default',
  [EntryType.Link]: 'primary',
  [EntryType.Code]: 'secondary',
  [EntryType.Task]: 'warning',
};

export const SearchPage: React.FC = () => {
  const navigate = useNavigate();
  const { projects } = useAppSelector((state) => state.projects);
  
  const [searchQuery, setSearchQuery] = useState('');
  const [debouncedQuery, setDebouncedQuery] = useState('');
  const [filters, setFilters] = useState<SearchFilters>({});
  const [showFilters, setShowFilters] = useState(false);
  const [results, setResults] = useState<PaginatedResponse<Entry> | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [searchHistory, setSearchHistory] = useState<string[]>([]);
  
  const searchInputRef = useRef<HTMLInputElement>(null);
  const pageSize = 20;

  // Load search history from localStorage
  useEffect(() => {
    const history = localStorage.getItem('searchHistory');
    if (history) {
      setSearchHistory(JSON.parse(history));
    }
  }, []);

  // Debounce search query
  useEffect(() => {
    const timer = setTimeout(() => {
      setDebouncedQuery(searchQuery);
      setPage(1);
    }, 300);

    return () => clearTimeout(timer);
  }, [searchQuery]);

  // Perform search when debounced query or filters change
  useEffect(() => {
    performSearch();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [debouncedQuery, filters, page]);

  const performSearch = useCallback(async () => {
    if (!debouncedQuery.trim() && Object.keys(filters).length === 0) {
      setResults(null);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const response = await searchApi.search({
        q: debouncedQuery || undefined,
        projectId: filters.projectId,
        type: filters.type,
        fromDate: filters.fromDate,
        toDate: filters.toDate,
        pageNumber: page,
        pageSize,
      });

      setResults(response.data);

      // Add to search history if query exists
      if (debouncedQuery.trim()) {
        const newHistory = [
          debouncedQuery,
          ...searchHistory.filter((h) => h !== debouncedQuery),
        ].slice(0, 10);
        setSearchHistory(newHistory);
        localStorage.setItem('searchHistory', JSON.stringify(newHistory));
      }
    } catch (err) {
      console.error('Search error:', err);
      setError('Failed to perform search. Please try again.');
      setResults(null);
    } finally {
      setLoading(false);
    }
  }, [debouncedQuery, filters, page, searchHistory]);

  const handleSearchChange = (_event: React.SyntheticEvent, value: string) => {
    setSearchQuery(value);
  };

  const handleClearSearch = () => {
    setSearchQuery('');
    setDebouncedQuery('');
    setFilters({});
    setResults(null);
    searchInputRef.current?.focus();
  };

  const handleFilterChange = (key: keyof SearchFilters, value: any) => {
    setFilters((prev) => ({
      ...prev,
      [key]: value || undefined,
    }));
    setPage(1);
  };

  const handleClearFilters = () => {
    setFilters({});
    setPage(1);
  };

  const handlePageChange = (_event: React.ChangeEvent<unknown>, value: number) => {
    setPage(value);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  };

  const highlightText = (text: string, query: string): React.ReactNode => {
    if (!query.trim()) return text;

    const parts = text.split(new RegExp(`(${query})`, 'gi'));
    return parts.map((part, index) =>
      part.toLowerCase() === query.toLowerCase() ? (
        <mark key={index} style={{ backgroundColor: '#fff59d', padding: '0 2px' }}>
          {part}
        </mark>
      ) : (
        <span key={index}>{part}</span>
      )
    );
  };

  const getEntrySnippet = (entry: Entry): string => {
    return entry.description || entry.content?.substring(0, 200) || entry.url || '';
  };

  const activeFilterCount = Object.values(filters).filter(Boolean).length;

  return (
    <Container maxWidth="lg" sx={{ py: 4 }}>
      {/* Search Bar */}
      <Paper elevation={2} sx={{ p: 3, mb: 3 }}>
        <Box sx={{ display: 'flex', gap: 2, alignItems: 'flex-start' }}>
          <Autocomplete
            freeSolo
            fullWidth
            options={searchHistory}
            value={searchQuery}
            onInputChange={handleSearchChange}
            renderInput={(params) => (
              <TextField
                {...params}
                inputRef={searchInputRef}
                placeholder="Search entries, links, notes... (English or Russian)"
                InputProps={{
                  ...params.InputProps,
                  startAdornment: (
                    <InputAdornment position="start">
                      <SearchIcon />
                    </InputAdornment>
                  ),
                  endAdornment: (
                    <>
                      {loading && <CircularProgress size={20} />}
                      {params.InputProps.endAdornment}
                    </>
                  ),
                }}
              />
            )}
          />
          <IconButton
            onClick={() => setShowFilters(!showFilters)}
            color={activeFilterCount > 0 ? 'primary' : 'default'}
            sx={{ mt: 1 }}
          >
            <FilterIcon />
            {activeFilterCount > 0 && (
              <Typography variant="caption" sx={{ position: 'absolute', top: 0, right: 0, bgcolor: 'primary.main', color: 'white', borderRadius: '50%', width: 20, height: 20, display: 'flex', alignItems: 'center', justifyContent: 'center' }}>
                {activeFilterCount}
              </Typography>
            )}
          </IconButton>
          {(searchQuery || activeFilterCount > 0) && (
            <IconButton onClick={handleClearSearch} sx={{ mt: 1 }}>
              <ClearIcon />
            </IconButton>
          )}
        </Box>

        {/* Filters Panel */}
        <Collapse in={showFilters}>
          <Box sx={{ mt: 3, pt: 3, borderTop: 1, borderColor: 'divider' }}>
            <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 2 }}>
              <FormControl sx={{ minWidth: 200, flex: '1 1 200px' }} size="small">
                <InputLabel>Project</InputLabel>
                <Select
                  value={filters.projectId || ''}
                  onChange={(e: SelectChangeEvent<number | string>) =>
                    handleFilterChange('projectId', e.target.value ? Number(e.target.value) : undefined)
                  }
                  label="Project"
                >
                  <MenuItem value="">All Projects</MenuItem>
                  {projects.map((project) => (
                    <MenuItem key={project.id} value={project.id}>
                      {project.name}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <FormControl sx={{ minWidth: 150, flex: '1 1 150px' }} size="small">
                <InputLabel>Type</InputLabel>
                <Select
                  value={filters.type ?? ''}
                  onChange={(e: SelectChangeEvent<EntryType | string>) =>
                    handleFilterChange('type', e.target.value !== '' ? Number(e.target.value) as EntryType : undefined)
                  }
                  label="Type"
                >
                  <MenuItem value="">All Types</MenuItem>
                  {Object.entries(ENTRY_TYPE_LABELS).map(([value, label]) => (
                    <MenuItem key={value} value={Number(value)}>
                      {label}
                    </MenuItem>
                  ))}
                </Select>
              </FormControl>

              <TextField
                sx={{ minWidth: 200, flex: '1 1 200px' }}
                size="small"
                type="date"
                label="From Date"
                value={filters.fromDate || ''}
                onChange={(e) => handleFilterChange('fromDate', e.target.value)}
                InputLabelProps={{ shrink: true }}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <CalendarIcon fontSize="small" />
                    </InputAdornment>
                  ),
                }}
              />

              <TextField
                sx={{ minWidth: 200, flex: '1 1 200px' }}
                size="small"
                type="date"
                label="To Date"
                value={filters.toDate || ''}
                onChange={(e) => handleFilterChange('toDate', e.target.value)}
                InputLabelProps={{ shrink: true }}
                InputProps={{
                  startAdornment: (
                    <InputAdornment position="start">
                      <CalendarIcon fontSize="small" />
                    </InputAdornment>
                  ),
                }}
              />
            </Box>

            {activeFilterCount > 0 && (
              <Box sx={{ mt: 2, display: 'flex', justifyContent: 'flex-end' }}>
                <Button size="small" onClick={handleClearFilters}>
                  Clear All Filters
                </Button>
              </Box>
            )}
          </Box>
        </Collapse>
      </Paper>

      {/* Active Filters Chips */}
      {activeFilterCount > 0 && (
        <Box sx={{ mb: 2, display: 'flex', gap: 1, flexWrap: 'wrap' }}>
          {filters.projectId && (
            <Chip
              label={`Project: ${projects.find((p) => p.id === filters.projectId)?.name || filters.projectId}`}
              onDelete={() => handleFilterChange('projectId', undefined)}
              size="small"
            />
          )}
          {filters.type !== undefined && (
            <Chip
              label={`Type: ${ENTRY_TYPE_LABELS[filters.type]}`}
              onDelete={() => handleFilterChange('type', undefined)}
              size="small"
            />
          )}
          {filters.fromDate && (
            <Chip
              label={`From: ${filters.fromDate}`}
              onDelete={() => handleFilterChange('fromDate', undefined)}
              size="small"
            />
          )}
          {filters.toDate && (
            <Chip
              label={`To: ${filters.toDate}`}
              onDelete={() => handleFilterChange('toDate', undefined)}
              size="small"
            />
          )}
        </Box>
      )}

      {/* Results Section */}
      {error && (
        <Alert severity="error" sx={{ mb: 3 }}>
          {error}
        </Alert>
      )}

      {results && (
        <>
          {/* Results Count */}
          <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
            Found {results.totalCount} {results.totalCount === 1 ? 'result' : 'results'}
            {debouncedQuery && ` for "${debouncedQuery}"`}
          </Typography>

          {/* Results List */}
          {results.items.length === 0 ? (
            <Paper sx={{ p: 4, textAlign: 'center' }}>
              <Typography variant="h6" color="text.secondary" gutterBottom>
                No results found
              </Typography>
              <Typography variant="body2" color="text.secondary">
                Try adjusting your search query or filters
              </Typography>
            </Paper>
          ) : (
            <Stack spacing={2}>
              {results.items.map((entry) => (
                <Card key={entry.id} variant="outlined">
                  <CardContent>
                    <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', mb: 1 }}>
                      <Typography variant="h6" component="div">
                        {highlightText(entry.title, debouncedQuery)}
                      </Typography>
                      <Chip
                        label={ENTRY_TYPE_LABELS[entry.type]}
                        color={ENTRY_TYPE_COLORS[entry.type]}
                        size="small"
                      />
                    </Box>

                    {entry.projectName && (
                      <Typography variant="body2" color="text.secondary" gutterBottom>
                        Project: {entry.projectName}
                      </Typography>
                    )}

                    {getEntrySnippet(entry) && (
                      <Typography variant="body2" color="text.secondary" sx={{ mb: 1 }}>
                        {highlightText(getEntrySnippet(entry), debouncedQuery)}
                      </Typography>
                    )}

                    {entry.url && (
                      <Typography variant="body2" color="primary" sx={{ mb: 1, wordBreak: 'break-all' }}>
                        {highlightText(entry.url, debouncedQuery)}
                      </Typography>
                    )}

                    {entry.tagNames && entry.tagNames.length > 0 && (
                      <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap', mt: 1 }}>
                        {entry.tagNames.map((tag) => (
                          <Chip key={tag} label={tag} size="small" variant="outlined" />
                        ))}
                      </Box>
                    )}

                    <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
                      {new Date(entry.updatedAt).toLocaleDateString()}
                    </Typography>
                  </CardContent>

                  <CardActions>
                    <Button size="small" onClick={() => navigate(`/entries/${entry.id}`)}>
                      View Details
                    </Button>
                    {entry.url && (
                      <Button
                        size="small"
                        href={entry.url}
                        target="_blank"
                        rel="noopener noreferrer"
                        endIcon={<OpenInNewIcon fontSize="small" />}
                      >
                        Open Link
                      </Button>
                    )}
                  </CardActions>
                </Card>
              ))}
            </Stack>
          )}

          {/* Pagination */}
          {results.totalPages > 1 && (
            <Box sx={{ mt: 4, display: 'flex', justifyContent: 'center' }}>
              <Pagination
                count={results.totalPages}
                page={page}
                onChange={handlePageChange}
                color="primary"
                showFirstButton
                showLastButton
              />
            </Box>
          )}
        </>
      )}

      {/* Empty State */}
      {!results && !loading && !debouncedQuery && Object.keys(filters).length === 0 && (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <SearchIcon sx={{ fontSize: 64, color: 'text.secondary', mb: 2 }} />
          <Typography variant="h6" color="text.secondary" gutterBottom>
            Start Searching
          </Typography>
          <Typography variant="body2" color="text.secondary">
            Enter a search query or apply filters to find entries
          </Typography>
          {searchHistory.length > 0 && (
            <Box sx={{ mt: 3 }}>
              <Typography variant="body2" color="text.secondary" gutterBottom>
                Recent Searches:
              </Typography>
              <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap', justifyContent: 'center', mt: 1 }}>
                {searchHistory.slice(0, 5).map((query) => (
                  <Chip
                    key={query}
                    label={query}
                    size="small"
                    onClick={() => setSearchQuery(query)}
                    clickable
                  />
                ))}
              </Box>
            </Box>
          )}
        </Paper>
      )}
    </Container>
  );
};
