import {
  Card,
  CardContent,
  CardActions,
  Typography,
  IconButton,
  Chip,
  Box,
  Tooltip,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import FolderIcon from '@mui/icons-material/Folder';
import { Project } from '../../types/models';
import { useNavigate } from 'react-router-dom';
import styles from './ProjectCard.module.css';

interface ProjectCardProps {
  project: Project;
  onEdit: (project: Project) => void;
  onDelete: (project: Project) => void;
}

export const ProjectCard = ({ project, onEdit, onDelete }: ProjectCardProps) => {
  const navigate = useNavigate();

  const handleCardClick = () => {
    navigate(`/projects/${project.id}`);
  };

  const handleEdit = (e: React.MouseEvent) => {
    e.stopPropagation();
    onEdit(project);
  };

  const handleDelete = (e: React.MouseEvent) => {
    e.stopPropagation();
    onDelete(project);
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
      <CardContent>
        <Box sx={{ display: 'flex', alignItems: 'flex-start', mb: 2 }}>
          <FolderIcon color="primary" sx={{ mr: 1, mt: 0.5 }} />
          <Box sx={{ flexGrow: 1 }}>
            <Typography variant="h6" component="h2" gutterBottom>
              {project.name}
            </Typography>
            {project.description && (
              <Typography
                variant="body2"
                color="text.secondary"
                sx={{
                  overflow: 'hidden',
                  textOverflow: 'ellipsis',
                  display: '-webkit-box',
                  WebkitLineClamp: 2,
                  WebkitBoxOrient: 'vertical',
                }}
              >
                {project.description}
              </Typography>
            )}
          </Box>
        </Box>

        {project.tags && project.tags.length > 0 && (
          <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5, mb: 1 }}>
            {project.tags.map(tag => (
              <Chip key={tag} label={tag} size="small" variant="outlined" />
            ))}
          </Box>
        )}

        <Typography variant="caption" color="text.secondary" display="block">
          Updated: {new Date(project.updatedAt).toLocaleDateString()}
        </Typography>
      </CardContent>

      <CardActions sx={{ justifyContent: 'flex-end', pt: 0 }}>
        <Tooltip title="Edit project">
          <IconButton size="small" color="primary" onClick={handleEdit}>
            <EditIcon fontSize="small" />
          </IconButton>
        </Tooltip>
        <Tooltip title="Delete project">
          <IconButton size="small" color="error" onClick={handleDelete}>
            <DeleteIcon fontSize="small" />
          </IconButton>
        </Tooltip>
      </CardActions>
    </Card>
  );
};
