import React, { useState, useEffect } from 'react';
import {
  Container,
  Paper,
  Typography,
  Box,
  Grid,
  Card,
  CardContent,
  TextField,
  Button,
  Chip,
  IconButton,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  List,
  ListItem,
  ListItemText,
  ListItemSecondaryAction,
  Alert,
  CircularProgress,
  Divider,
  Stack
} from '@mui/material';
import {
  Delete as DeleteIcon,
  Edit as EditIcon,
  MergeType as MergeIcon,
  Search as SearchIcon,
  TrendingUp as TrendingUpIcon,
  Schedule as ScheduleIcon,
  Block as BlockIcon
} from '@mui/icons-material';
import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5182';

interface Tag {
  id: string;
  name: string;
  usageCount: number;
  createdAt: string;
}

interface TagStatistics {
  totalTags: number;
  totalUsages: number;
  mostUsedTags: Tag[];
  recentlyUsedTags: Tag[];
  unusedTags: Tag[];
}

const TagManagement: React.FC = () => {
  const [tags, setTags] = useState<Tag[]>([]);
  const [statistics, setStatistics] = useState<TagStatistics | null>(null);
  const [loading, setLoading] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');
  const [selectedTag, setSelectedTag] = useState<Tag | null>(null);
  const [renameDialogOpen, setRenameDialogOpen] = useState(false);
  const [mergeDialogOpen, setMergeDialogOpen] = useState(false);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);
  const [newName, setNewName] = useState('');
  const [targetTagId, setTargetTagId] = useState('');
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  useEffect(() => {
    fetchTags();
    fetchStatistics();
  }, [searchQuery]);

  const fetchTags = async () => {
    setLoading(true);
    try {
      const endpoint = searchQuery
        ? `${API_BASE_URL}/api/tags?search=${encodeURIComponent(searchQuery)}`
        : `${API_BASE_URL}/api/tags`;
      const response = await axios.get(endpoint);
      setTags(response.data);
      setError(null);
    } catch (err) {
      setError('Failed to fetch tags');
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const fetchStatistics = async () => {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/tags/statistics`);
      setStatistics(response.data);
    } catch (err) {
      console.error('Failed to fetch statistics:', err);
    }
  };

  const handleRename = async () => {
    if (!selectedTag || !newName.trim()) return;

    try {
      await axios.put(`${API_BASE_URL}/api/tags/${selectedTag.id}/rename`, {
        newName: newName.trim()
      });
      setSuccess(`Tag renamed to "${newName.trim()}"`);
      setRenameDialogOpen(false);
      setNewName('');
      setSelectedTag(null);
      fetchTags();
      fetchStatistics();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to rename tag');
    }
  };

  const handleMerge = async () => {
    if (!selectedTag || !targetTagId) return;

    try {
      await axios.post(`${API_BASE_URL}/api/tags/merge`, {
        sourceTagId: selectedTag.id,
        targetTagId: targetTagId
      });
      setSuccess(`Tag "${selectedTag.name}" merged successfully`);
      setMergeDialogOpen(false);
      setTargetTagId('');
      setSelectedTag(null);
      fetchTags();
      fetchStatistics();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to merge tags');
    }
  };

  const handleDelete = async () => {
    if (!selectedTag) return;

    try {
      await axios.delete(`${API_BASE_URL}/api/tags/${selectedTag.id}`);
      setSuccess(`Tag "${selectedTag.name}" deleted`);
      setDeleteDialogOpen(false);
      setSelectedTag(null);
      fetchTags();
      fetchStatistics();
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to delete tag');
    }
  };

  const openRenameDialog = (tag: Tag) => {
    setSelectedTag(tag);
    setNewName(tag.name);
    setRenameDialogOpen(true);
  };

  const openMergeDialog = (tag: Tag) => {
    setSelectedTag(tag);
    setMergeDialogOpen(true);
  };

  const openDeleteDialog = (tag: Tag) => {
    setSelectedTag(tag);
    setDeleteDialogOpen(true);
  };

  return (
    <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
      <Typography variant="h4" gutterBottom>
        Tag Management
      </Typography>

      {error && (
        <Alert severity="error" onClose={() => setError(null)} sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      {success && (
        <Alert severity="success" onClose={() => setSuccess(null)} sx={{ mb: 2 }}>
          {success}
        </Alert>
      )}

      {/* Statistics */}
      {statistics && (
        <Grid container spacing={2} sx={{ mb: 4 }}>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Total Tags
                </Typography>
                <Typography variant="h4">{statistics.totalTags}</Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Total Usages
                </Typography>
                <Typography variant="h4">{statistics.totalUsages}</Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Most Used
                </Typography>
                <Typography variant="h6">
                  {statistics.mostUsedTags[0]?.name || 'N/A'}
                </Typography>
                <Typography variant="body2" color="textSecondary">
                  {statistics.mostUsedTags[0]?.usageCount || 0} uses
                </Typography>
              </CardContent>
            </Card>
          </Grid>
          <Grid item xs={12} sm={6} md={3}>
            <Card>
              <CardContent>
                <Typography color="textSecondary" gutterBottom>
                  Unused Tags
                </Typography>
                <Typography variant="h4">{statistics.unusedTags.length}</Typography>
              </CardContent>
            </Card>
          </Grid>
        </Grid>
      )}

      {/* Search */}
      <Paper sx={{ p: 2, mb: 3 }}>
        <TextField
          fullWidth
          placeholder="Search tags..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          InputProps={{
            startAdornment: <SearchIcon sx={{ mr: 1, color: 'text.secondary' }} />
          }}
        />
      </Paper>

      {/* Tags List */}
      <Paper sx={{ p: 2 }}>
        <Typography variant="h6" gutterBottom>
          All Tags ({tags.length})
        </Typography>
        <Divider sx={{ mb: 2 }} />

        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
            <CircularProgress />
          </Box>
        ) : tags.length === 0 ? (
          <Typography color="textSecondary" align="center" sx={{ py: 4 }}>
            No tags found
          </Typography>
        ) : (
          <List>
            {tags.map((tag) => (
              <ListItem key={tag.id} divider>
                <ListItemText
                  primary={
                    <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                      <Chip label={tag.name} color="primary" size="small" />
                      <Chip
                        label={`${tag.usageCount} ${tag.usageCount === 1 ? 'use' : 'uses'}`}
                        size="small"
                        variant="outlined"
                      />
                    </Box>
                  }
                  secondary={`Created: ${new Date(tag.createdAt).toLocaleDateString()}`}
                />
                <ListItemSecondaryAction>
                  <IconButton
                    edge="end"
                    aria-label="rename"
                    onClick={() => openRenameDialog(tag)}
                    sx={{ mr: 1 }}
                  >
                    <EditIcon />
                  </IconButton>
                  <IconButton
                    edge="end"
                    aria-label="merge"
                    onClick={() => openMergeDialog(tag)}
                    sx={{ mr: 1 }}
                  >
                    <MergeIcon />
                  </IconButton>
                  <IconButton
                    edge="end"
                    aria-label="delete"
                    onClick={() => openDeleteDialog(tag)}
                    color="error"
                  >
                    <DeleteIcon />
                  </IconButton>
                </ListItemSecondaryAction>
              </ListItem>
            ))}
          </List>
        )}
      </Paper>

      {/* Quick Stats Sections */}
      {statistics && (
        <Grid container spacing={2} sx={{ mt: 2 }}>
          <Grid item xs={12} md={4}>
            <Paper sx={{ p: 2 }}>
              <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
                <TrendingUpIcon color="primary" />
                <Typography variant="h6">Most Used</Typography>
              </Stack>
              <Stack spacing={1}>
                {statistics.mostUsedTags.slice(0, 5).map((tag) => (
                  <Chip
                    key={tag.id}
                    label={`${tag.name} (${tag.usageCount})`}
                    size="small"
                    color="primary"
                    variant="outlined"
                  />
                ))}
              </Stack>
            </Paper>
          </Grid>

          <Grid item xs={12} md={4}>
            <Paper sx={{ p: 2 }}>
              <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
                <ScheduleIcon color="secondary" />
                <Typography variant="h6">Recently Used</Typography>
              </Stack>
              <Stack spacing={1}>
                {statistics.recentlyUsedTags.slice(0, 5).map((tag) => (
                  <Chip
                    key={tag.id}
                    label={`${tag.name} (${tag.usageCount})`}
                    size="small"
                    color="secondary"
                    variant="outlined"
                  />
                ))}
              </Stack>
            </Paper>
          </Grid>

          <Grid item xs={12} md={4}>
            <Paper sx={{ p: 2 }}>
              <Stack direction="row" alignItems="center" spacing={1} sx={{ mb: 2 }}>
                <BlockIcon color="error" />
                <Typography variant="h6">Unused Tags</Typography>
              </Stack>
              <Stack spacing={1}>
                {statistics.unusedTags.slice(0, 5).map((tag) => (
                  <Chip
                    key={tag.id}
                    label={tag.name}
                    size="small"
                    color="error"
                    variant="outlined"
                    onDelete={() => openDeleteDialog(tag)}
                  />
                ))}
              </Stack>
            </Paper>
          </Grid>
        </Grid>
      )}

      {/* Rename Dialog */}
      <Dialog open={renameDialogOpen} onClose={() => setRenameDialogOpen(false)}>
        <DialogTitle>Rename Tag</DialogTitle>
        <DialogContent>
          <TextField
            autoFocus
            margin="dense"
            label="New Tag Name"
            fullWidth
            value={newName}
            onChange={(e) => setNewName(e.target.value)}
          />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setRenameDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleRename} variant="contained">
            Rename
          </Button>
        </DialogActions>
      </Dialog>

      {/* Merge Dialog */}
      <Dialog open={mergeDialogOpen} onClose={() => setMergeDialogOpen(false)}>
        <DialogTitle>Merge Tag</DialogTitle>
        <DialogContent>
          <Typography variant="body2" sx={{ mb: 2 }}>
            Merge "{selectedTag?.name}" into another tag. All entries with this tag will be updated.
          </Typography>
          <TextField
            select
            fullWidth
            label="Target Tag"
            value={targetTagId}
            onChange={(e) => setTargetTagId(e.target.value)}
            SelectProps={{ native: true }}
          >
            <option value="">Select a tag...</option>
            {tags
              .filter((t) => t.id !== selectedTag?.id)
              .map((tag) => (
                <option key={tag.id} value={tag.id}>
                  {tag.name} ({tag.usageCount} uses)
                </option>
              ))}
          </TextField>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setMergeDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleMerge} variant="contained" disabled={!targetTagId}>
            Merge
          </Button>
        </DialogActions>
      </Dialog>

      {/* Delete Dialog */}
      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Delete Tag</DialogTitle>
        <DialogContent>
          <Typography>
            Are you sure you want to delete the tag "{selectedTag?.name}"?
            {selectedTag && selectedTag.usageCount > 0 && (
              <Alert severity="warning" sx={{ mt: 2 }}>
                This tag is used in {selectedTag.usageCount} {selectedTag.usageCount === 1 ? 'entry' : 'entries'}.
              </Alert>
            )}
          </Typography>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleDelete} color="error" variant="contained">
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </Container>
  );
};

export default TagManagement;
