import React from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Card,
  CardContent,
  CardActions,
  Typography,
  IconButton,
  Tooltip,
  Chip,
  Box,
} from '@mui/material';
import {
  Edit as EditIcon,
  Delete as DeleteIcon,
  Link as LinkIcon,
  Description as NoteIcon,
  Code as CodeIcon,
  CheckCircle as TaskIcon,
} from '@mui/icons-material';
import { Entry, EntryType } from '../../types/models';
import styles from './EntryCard.module.css';

interface EntryCardProps {
  entry: Entry;
  onEdit: (entry: Entry) => void;
  onDelete: (entry: Entry) => void;
}

const EntryCard: React.FC<EntryCardProps> = ({ entry, onEdit, onDelete }) => {
  const navigate = useNavigate();

  const getTypeIcon = () => {
    switch (entry.type) {
      case EntryType.Link:
        return <LinkIcon color="primary" />;
      case EntryType.Note:
        return <NoteIcon color="success" />;
      case EntryType.Code:
        return <CodeIcon color="warning" />;
      case EntryType.Task:
        return <TaskIcon color="info" />;
      default:
        return <NoteIcon />;
    }
  };

  const getTypeLabel = () => {
    switch (entry.type) {
      case EntryType.Link:
        return 'Link';
      case EntryType.Note:
        return 'Note';
      case EntryType.Code:
        return 'Code';
      case EntryType.Task:
        return 'Task';
      default:
        return 'Unknown';
    }
  };

  const getTypeColor = () => {
    switch (entry.type) {
      case EntryType.Link:
        return 'primary';
      case EntryType.Note:
        return 'success';
      case EntryType.Code:
        return 'warning';
      case EntryType.Task:
        return 'info';
      default:
        return 'default';
    }
  };

  const handleCardClick = () => {
    navigate(`/entries/${entry.id}`);
  };

  const handleEdit = (e: React.MouseEvent) => {
    e.stopPropagation();
    onEdit(entry);
  };

  const handleDelete = (e: React.MouseEvent) => {
    e.stopPropagation();
    onDelete(entry);
  };

  return (
    <Card
      className={styles.card}
      onClick={handleCardClick}
      sx={{
        cursor: 'pointer',
        transition: 'all 0.2s',
        '&:hover': {
          transform: 'translateY(-4px)',
          boxShadow: 4,
        },
      }}
    >
      <CardContent sx={{ flexGrow: 1 }}>
        <Box display="flex" alignItems="center" gap={1} mb={1}>
          {getTypeIcon()}
          <Chip label={getTypeLabel()} size="small" color={getTypeColor() as any} />
        </Box>

        <Typography variant="h6" component="h3" gutterBottom noWrap>
          {entry.title}
        </Typography>

        {entry.url && (
          <Typography variant="body2" color="text.secondary" noWrap gutterBottom>
            {entry.url}
          </Typography>
        )}

        {entry.content && (
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              display: '-webkit-box',
              WebkitLineClamp: 3,
              WebkitBoxOrient: 'vertical',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              mb: 1,
            }}
          >
            {entry.content}
          </Typography>
        )}

        {entry.metadata && (
          <Box mt={1}>
            {entry.metadata.faviconUrl && (
              <Box display="flex" alignItems="center" gap={1} mb={0.5}>
                <img
                  src={entry.metadata.faviconUrl}
                  alt="Favicon"
                  style={{ width: 16, height: 16 }}
                />
                <Typography variant="caption" color="text.secondary">
                  {entry.metadata.siteName || 'Website'}
                </Typography>
              </Box>
            )}
            {entry.metadata.description && (
              <Typography
                variant="caption"
                color="text.secondary"
                sx={{
                  display: '-webkit-box',
                  WebkitLineClamp: 2,
                  WebkitBoxOrient: 'vertical',
                  overflow: 'hidden',
                }}
              >
                {entry.metadata.description}
              </Typography>
            )}
          </Box>
        )}

        {entry.tags && entry.tags.length > 0 && (
          <Box mt={1} display="flex" gap={0.5} flexWrap="wrap">
            {entry.tags.map((tag) => (
              <Chip key={tag} label={tag} size="small" variant="outlined" />
            ))}
          </Box>
        )}

        <Typography variant="caption" color="text.secondary" display="block" mt={1}>
          Updated: {new Date(entry.updatedAt).toLocaleDateString()}
        </Typography>
      </CardContent>

      <CardActions sx={{ justifyContent: 'flex-end', p: 1 }}>
        <Tooltip title="Edit">
          <IconButton size="small" onClick={handleEdit} color="primary">
            <EditIcon fontSize="small" />
          </IconButton>
        </Tooltip>
        <Tooltip title="Delete">
          <IconButton size="small" onClick={handleDelete} color="error">
            <DeleteIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      </CardActions>
    </Card>
  );
};

export default EntryCard;
