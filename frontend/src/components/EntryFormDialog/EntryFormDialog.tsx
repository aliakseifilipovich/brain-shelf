import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  MenuItem,
  Chip,
  Box,
  Typography,
  CircularProgress,
} from '@mui/material';
import { Entry, EntryType } from '../../types/models';

interface EntryFormDialogProps {
  open: boolean;
  entry: Entry | null;
  projectId?: number;
  onClose: () => void;
  onSubmit: (entryData: Partial<Entry>) => Promise<void>;
}

const EntryFormDialog: React.FC<EntryFormDialogProps> = ({
  open,
  entry,
  projectId,
  onClose,
  onSubmit,
}) => {
  const [title, setTitle] = useState('');
  const [content, setContent] = useState('');
  const [url, setUrl] = useState('');
  const [type, setType] = useState<EntryType>(EntryType.Note);
  const [selectedProjectId, setSelectedProjectId] = useState<number | undefined>(projectId);
  const [tags, setTags] = useState<string[]>([]);
  const [tagInput, setTagInput] = useState('');
  const [errors, setErrors] = useState<Record<string, string>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (entry) {
      setTitle(entry.title);
      setContent(entry.content || '');
      setUrl(entry.url || '');
      setType(entry.type);
      setSelectedProjectId(entry.projectId);
      setTags(entry.tags || []);
    } else {
      setTitle('');
      setContent('');
      setUrl('');
      setType(EntryType.Note);
      setSelectedProjectId(projectId);
      setTags([]);
    }
    setErrors({});
  }, [entry, projectId, open]);

  const validate = (): boolean => {
    const newErrors: Record<string, string> = {};

    if (!title.trim()) {
      newErrors.title = 'Title is required';
    } else if (title.length < 3 || title.length > 200) {
      newErrors.title = 'Title must be between 3 and 200 characters';
    }

    if (type === EntryType.Link) {
      if (!url.trim()) {
        newErrors.url = 'URL is required for link entries';
      } else {
        try {
          new URL(url);
        } catch {
          newErrors.url = 'Please enter a valid URL';
        }
      }
    }

    if (content && content.length > 10000) {
      newErrors.content = 'Content must be less than 10000 characters';
    }

    if (!selectedProjectId) {
      newErrors.projectId = 'Project is required';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleSubmit = async () => {
    if (!validate()) {
      return;
    }

    setIsSubmitting(true);
    try {
      const entryData: Partial<Entry> = {
        title: title.trim(),
        content: content.trim() || undefined,
        url: type === EntryType.Link ? url.trim() : undefined,
        type,
        projectId: selectedProjectId!,
        tags: tags.length > 0 ? tags : undefined,
      };

      if (entry) {
        entryData.id = entry.id;
      }

      await onSubmit(entryData);
      onClose();
    } catch (error) {
      console.error('Failed to submit entry:', error);
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleAddTag = () => {
    const trimmedTag = tagInput.trim();
    if (trimmedTag && !tags.includes(trimmedTag)) {
      setTags([...tags, trimmedTag]);
      setTagInput('');
    }
  };

  const handleDeleteTag = (tagToDelete: string) => {
    setTags(tags.filter((tag) => tag !== tagToDelete));
  };

  const handleTagKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      handleAddTag();
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{entry ? 'Edit Entry' : 'Create New Entry'}</DialogTitle>
      <DialogContent>
        <Box display="flex" flexDirection="column" gap={2} mt={1}>
          <TextField
            label="Entry Type"
            select
            value={type}
            onChange={(e) => setType(Number(e.target.value) as EntryType)}
            fullWidth
            size="small"
          >
            <MenuItem value={EntryType.Note}>Note</MenuItem>
            <MenuItem value={EntryType.Link}>Link</MenuItem>
            <MenuItem value={EntryType.Code}>Code</MenuItem>
            <MenuItem value={EntryType.Task}>Task</MenuItem>
          </TextField>

          <TextField
            label="Title"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            error={!!errors.title}
            helperText={errors.title || 'Required (3-200 characters)'}
            fullWidth
            size="small"
            required
          />

          {type === EntryType.Link && (
            <TextField
              label="URL"
              value={url}
              onChange={(e) => setUrl(e.target.value)}
              error={!!errors.url}
              helperText={errors.url || 'Enter a valid URL'}
              fullWidth
              size="small"
              required
              placeholder="https://example.com"
            />
          )}

          <TextField
            label="Content"
            value={content}
            onChange={(e) => setContent(e.target.value)}
            error={!!errors.content}
            helperText={
              errors.content ||
              `Optional (max 10000 characters) ${content.length}/10000`
            }
            fullWidth
            multiline
            rows={6}
            size="small"
            placeholder={
              type === EntryType.Code
                ? 'Paste your code here...'
                : type === EntryType.Task
                ? 'Describe the task...'
                : 'Enter your note content...'
            }
          />

          <Box>
            <TextField
              label="Add Tags"
              value={tagInput}
              onChange={(e) => setTagInput(e.target.value)}
              onKeyPress={handleTagKeyPress}
              fullWidth
              size="small"
              placeholder="Type and press Enter to add tags"
            />
            {tags.length > 0 && (
              <Box mt={1} display="flex" gap={0.5} flexWrap="wrap">
                {tags.map((tag) => (
                  <Chip
                    key={tag}
                    label={tag}
                    onDelete={() => handleDeleteTag(tag)}
                    size="small"
                  />
                ))}
              </Box>
            )}
          </Box>

          {type === EntryType.Link && url && (
            <Typography variant="caption" color="text.secondary">
              Metadata will be automatically extracted after saving
            </Typography>
          )}
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose} disabled={isSubmitting}>
          Cancel
        </Button>
        <Button
          onClick={handleSubmit}
          variant="contained"
          disabled={isSubmitting}
          startIcon={isSubmitting ? <CircularProgress size={16} /> : null}
        >
          {isSubmitting ? 'Saving...' : entry ? 'Update' : 'Create'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default EntryFormDialog;
