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
        height: '100%',
        display: 'flex',
        flexDirection: 'column',
        transition: 'all 0.2s ease-in-out',
        border: '1px solid',
        borderColor: 'divider',
        '&:hover': {
          transform: 'translateY(-2px)',
          boxShadow: 3,
          borderColor: 'primary.light',
        },
      }}
    >
      <CardContent sx={{ flexGrow: 1, p: 2 }}>
        <Box display="flex" alignItems="center" gap={1} mb={1.5}>
          <Box sx={{ 
            display: 'flex', 
            alignItems: 'center', 
            justifyContent: 'center',
            width: 24,
            height: 24,
            borderRadius: 0.5,
            backgroundColor: `${getTypeColor()}.50`,
          }}>
            {getTypeIcon()}
          </Box>
          <Chip 
            label={getTypeLabel()} 
            size="small" 
            color={getTypeColor() as any}
            sx={{ height: 20, fontSize: '0.7rem', fontWeight: 500 }}
          />
        </Box>

        <Typography 
          variant="h6" 
          component="h3" 
          gutterBottom 
          noWrap
          sx={{ 
            fontSize: '1rem',
            fontWeight: 600,
            mb: 1,
          }}
        >
          {entry.title}
        </Typography>

        {entry.url && (
          <Typography 
            variant="body2" 
            color="text.secondary" 
            noWrap 
            sx={{ 
              fontSize: '0.75rem',
              mb: 1,
              opacity: 0.7,
            }}
          >
            {entry.url}
          </Typography>
        )}

        {entry.content && (
          <Typography
            variant="body2"
            color="text.secondary"
            sx={{
              display: '-webkit-box',
              WebkitLineClamp: 2,
              WebkitBoxOrient: 'vertical',
              overflow: 'hidden',
              textOverflow: 'ellipsis',
              fontSize: '0.8rem',
              lineHeight: 1.5,
              mb: 1.5,
            }}
          >
            {entry.content}
          </Typography>
        )}

        {entry.metadata && entry.metadata.faviconUrl && (
          <Box display="flex" alignItems="center" gap={0.5} mb={1}>
            <img
              src={entry.metadata.faviconUrl}
              alt="Favicon"
              style={{ width: 14, height: 14 }}
            />
            <Typography variant="caption" color="text.secondary" sx={{ fontSize: '0.7rem' }}>
              {entry.metadata.siteName || 'Website'}
            </Typography>
          </Box>
        )}

        {entry.tags && entry.tags.length > 0 && (
          <Box mt={1.5} display="flex" gap={0.5} flexWrap="wrap">
            {entry.tags.slice(0, 3).map((tag) => (
              <Chip 
                key={tag} 
                label={tag} 
                size="small" 
                variant="outlined"
                sx={{ 
                  height: 18, 
                  fontSize: '0.65rem',
                  borderColor: 'divider',
                }}
              />
            ))}
            {entry.tags.length > 3 && (
              <Chip 
                label={`+${entry.tags.length - 3}`} 
                size="small" 
                variant="outlined"
                sx={{ 
                  height: 18, 
                  fontSize: '0.65rem',
                  borderColor: 'divider',
                }}
              />
            )}
          </Box>
        )}

        <Typography 
          variant="caption" 
          color="text.secondary" 
          display="block" 
          mt={2}
          sx={{ fontSize: '0.7rem', opacity: 0.6 }}
        >
          {new Date(entry.updatedAt).toLocaleDateString()}
        </Typography>
      </CardContent>

      <CardActions sx={{ justifyContent: 'flex-end', p: 1, pt: 0, gap: 0.5 }}>
        <Tooltip title="Edit">
          <IconButton 
            size="small" 
            onClick={handleEdit} 
            sx={{ 
              '&:hover': { 
                backgroundColor: 'primary.50',
                color: 'primary.main',
              }
            }}
          >
            <EditIcon sx={{ fontSize: 18 }} />
          </IconButton>
        </Tooltip>
        <Tooltip title="Delete">
          <IconButton 
            size="small" 
            onClick={handleDelete}
            sx={{ 
              '&:hover': { 
                backgroundColor: 'error.50',
                color: 'error.main',
              }
            }}
          >
            <DeleteIcon sx={{ fontSize: 18 }} />
          </IconButton>
        </Tooltip>
      </CardActions>
    </Card>
  );
};

export default EntryCard;
