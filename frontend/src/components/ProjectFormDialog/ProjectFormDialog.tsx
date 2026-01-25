import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Box,
  Chip,
  IconButton,
  FormHelperText,
} from '@mui/material';
import AddIcon from '@mui/icons-material/Add';
import { Project } from '../../types/models';

interface ProjectFormDialogProps {
  open: boolean;
  project?: Project | null;
  onClose: () => void;
  onSubmit: (data: Omit<Project, 'id' | 'createdAt' | 'updatedAt'>) => void;
}

export const ProjectFormDialog = ({ open, project, onClose, onSubmit }: ProjectFormDialogProps) => {
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [tags, setTags] = useState<string[]>([]);
  const [tagInput, setTagInput] = useState('');
  const [errors, setErrors] = useState<{ name?: string; tags?: string }>({});

  useEffect(() => {
    if (project) {
      setName(project.name || '');
      setDescription(project.description || '');
      setTags(project.tags || []);
    } else {
      setName('');
      setDescription('');
      setTags([]);
    }
    setTagInput('');
    setErrors({});
  }, [project, open]);

  const validate = () => {
    const newErrors: { name?: string; tags?: string } = {};

    if (!name.trim()) {
      newErrors.name = 'Project name is required';
    } else if (name.length < 3) {
      newErrors.name = 'Project name must be at least 3 characters';
    } else if (name.length > 100) {
      newErrors.name = 'Project name must be less than 100 characters';
    }

    if (description.length > 500) {
      newErrors.tags = 'Description must be less than 500 characters';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  const handleAddTag = () => {
    const trimmedTag = tagInput.trim();
    if (trimmedTag && !tags.includes(trimmedTag)) {
      setTags([...tags, trimmedTag]);
      setTagInput('');
    }
  };

  const handleDeleteTag = (tagToDelete: string) => {
    setTags(tags.filter(tag => tag !== tagToDelete));
  };

  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      handleAddTag();
    }
  };

  const handleSubmit = () => {
    if (validate()) {
      onSubmit({
        name: name.trim(),
        description: description.trim() || undefined,
        tags: tags.length > 0 ? tags : undefined,
      });
      onClose();
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>{project ? 'Edit Project' : 'Create New Project'}</DialogTitle>
      <DialogContent>
        <Box sx={{ mt: 2, display: 'flex', flexDirection: 'column', gap: 2 }}>
          <TextField
            label="Project Name"
            value={name}
            onChange={e => setName(e.target.value)}
            error={!!errors.name}
            helperText={errors.name}
            required
            fullWidth
            autoFocus
          />

          <TextField
            label="Description"
            value={description}
            onChange={e => setDescription(e.target.value)}
            multiline
            rows={3}
            fullWidth
          />

          <Box>
            <Box sx={{ display: 'flex', gap: 1, mb: 1 }}>
              <TextField
                label="Tags"
                value={tagInput}
                onChange={e => setTagInput(e.target.value)}
                onKeyPress={handleKeyPress}
                size="small"
                fullWidth
                placeholder="Add a tag and press Enter"
              />
              <IconButton color="primary" onClick={handleAddTag}>
                <AddIcon />
              </IconButton>
            </Box>

            {tags.length > 0 && (
              <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                {tags.map(tag => (
                  <Chip key={tag} label={tag} onDelete={() => handleDeleteTag(tag)} size="small" />
                ))}
              </Box>
            )}
            {errors.tags && <FormHelperText error>{errors.tags}</FormHelperText>}
          </Box>
        </Box>
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button onClick={handleSubmit} variant="contained" color="primary">
          {project ? 'Update' : 'Create'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};
