import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Typography,
  Box,
  Chip,
  Divider
} from '@mui/material';
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5182';

interface Template {
  id: string;
  name: string;
  description?: string;
  type: number;
  title?: string;
  content?: string;
  tags: string[];
  isDefault: boolean;
  projectId?: string;
}

interface TemplateSelectDialogProps {
  open: boolean;
  onClose: () => void;
  onSelect: (template: Template) => void;
  projectId?: string;
}

const TemplateSelectDialog: React.FC<TemplateSelectDialogProps> = ({
  open,
  onClose,
  onSelect,
  projectId
}) => {
  const [templates, setTemplates] = useState<Template[]>([]);
  const [selectedTemplateId, setSelectedTemplateId] = useState<string>('');
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    if (open) {
      fetchTemplates();
    }
  }, [open, projectId]);

  const fetchTemplates = async () => {
    setLoading(true);
    try {
      const endpoint = projectId
        ? `${API_BASE_URL}/api/templates?projectId=${projectId}`
        : `${API_BASE_URL}/api/templates/default`;
      const response = await axios.get(endpoint);
      setTemplates(response.data);
    } catch (error) {
      console.error('Error fetching templates:', error);
    } finally {
      setLoading(false);
    }
  };

  const handleSelect = () => {
    const template = templates.find(t => t.id === selectedTemplateId);
    if (template) {
      onSelect(template);
      setSelectedTemplateId('');
    }
  };

  const selectedTemplate = templates.find(t => t.id === selectedTemplateId);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle>Select Template</DialogTitle>
      <DialogContent>
        <FormControl fullWidth sx={{ mt: 2 }}>
          <InputLabel>Template</InputLabel>
          <Select
            value={selectedTemplateId}
            onChange={(e) => setSelectedTemplateId(e.target.value)}
            label="Template"
          >
            {templates.map((template) => (
              <MenuItem key={template.id} value={template.id}>
                {template.name}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        {selectedTemplate && (
          <Box sx={{ mt: 3 }}>
            <Divider sx={{ mb: 2 }} />
            <Typography variant="subtitle2" color="textSecondary">
              Preview
            </Typography>
            {selectedTemplate.description && (
              <Typography variant="body2" sx={{ mt: 1 }}>
                {selectedTemplate.description}
              </Typography>
            )}
            {selectedTemplate.title && (
              <Box sx={{ mt: 2 }}>
                <Typography variant="caption" color="textSecondary">
                  Title:
                </Typography>
                <Typography variant="body2">{selectedTemplate.title}</Typography>
              </Box>
            )}
            {selectedTemplate.tags.length > 0 && (
              <Box sx={{ mt: 2 }}>
                <Typography variant="caption" color="textSecondary" display="block">
                  Tags:
                </Typography>
                <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap', mt: 0.5 }}>
                  {selectedTemplate.tags.map((tag) => (
                    <Chip key={tag} label={tag} size="small" />
                  ))}
                </Box>
              </Box>
            )}
          </Box>
        )}
      </DialogContent>
      <DialogActions>
        <Button onClick={onClose}>Cancel</Button>
        <Button
          onClick={handleSelect}
          variant="contained"
          disabled={!selectedTemplateId}
        >
          Use Template
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default TemplateSelectDialog;
