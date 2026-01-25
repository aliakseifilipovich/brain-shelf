import React, { useState, useEffect, useCallback } from 'react';
import {
  Autocomplete,
  TextField,
  Chip,
  CircularProgress,
  Box
} from '@mui/material';
import { debounce } from '@mui/material/utils';
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5182';

interface Tag {
  id: string;
  name: string;
  usageCount: number;
}

interface TagAutocompleteProps {
  value: string[];
  onChange: (tags: string[]) => void;
  label?: string;
  placeholder?: string;
  showPopular?: boolean;
}

const TagAutocomplete: React.FC<TagAutocompleteProps> = ({
  value,
  onChange,
  label = 'Tags',
  placeholder = 'Add tags...',
  showPopular = true
}) => {
  const [options, setOptions] = useState<Tag[]>([]);
  const [loading, setLoading] = useState(false);
  const [inputValue, setInputValue] = useState('');

  // Fetch tags based on search query
  const fetchTags = useCallback(
    debounce(async (query: string) => {
      if (!query && !showPopular) {
        setOptions([]);
        return;
      }

      setLoading(true);
      try {
        const endpoint = query
          ? `${API_BASE_URL}/api/tags?search=${encodeURIComponent(query)}`
          : `${API_BASE_URL}/api/tags/popular?limit=10`;

        const response = await axios.get(endpoint);
        setOptions(response.data);
      } catch (error) {
        console.error('Error fetching tags:', error);
        setOptions([]);
      } finally {
        setLoading(false);
      }
    }, 300),
    [showPopular]
  );

  useEffect(() => {
    fetchTags(inputValue);
  }, [inputValue, fetchTags]);

  // Load popular tags on mount
  useEffect(() => {
    if (showPopular && !inputValue) {
      fetchTags('');
    }
  }, [showPopular, fetchTags]);

  const handleChange = (_event: any, newValue: string[]) => {
    // Normalize tag names (lowercase, trimmed)
    const normalizedTags = newValue.map(tag => tag.trim().toLowerCase());
    onChange(normalizedTags);
  };

  return (
    <Autocomplete
      multiple
      freeSolo
      options={options.map(tag => tag.name)}
      value={value}
      onChange={handleChange}
      inputValue={inputValue}
      onInputChange={(_event, newInputValue) => {
        setInputValue(newInputValue);
      }}
      loading={loading}
      renderTags={(tagValue, getTagProps) =>
        tagValue.map((option, index) => (
          <Chip
            label={option}
            {...getTagProps({ index })}
            color="primary"
            size="small"
          />
        ))
      }
      renderInput={(params) => (
        <TextField
          {...params}
          label={label}
          placeholder={value.length === 0 ? placeholder : ''}
          InputProps={{
            ...params.InputProps,
            endAdornment: (
              <>
                {loading ? <CircularProgress color="inherit" size={20} /> : null}
                {params.InputProps.endAdornment}
              </>
            ),
          }}
        />
      )}
      renderOption={(props, option) => {
        const tag = options.find(t => t.name === option);
        return (
          <Box component="li" {...props}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', width: '100%' }}>
              <span>{option}</span>
              {tag && tag.usageCount > 0 && (
                <Chip
                  label={tag.usageCount}
                  size="small"
                  sx={{ ml: 1, height: 20, fontSize: '0.7rem' }}
                />
              )}
            </Box>
          </Box>
        );
      }}
    />
  );
};

export default TagAutocomplete;
