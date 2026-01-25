import React, { useState } from 'react';
import {
  IconButton,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Button
} from '@mui/material';
import {
  MoreVert as MoreVertIcon,
  ContentCopy as DuplicateIcon,
  Delete as DeleteIcon,
  Edit as EditIcon
} from '@mui/icons-material';

interface EntryActionsMenuProps {
  entryId: string;
  entryTitle: string;
  onDuplicate: (entryId: string) => void;
  onDelete: (entryId: string) => void;
  onEdit: (entryId: string) => void;
}

const EntryActionsMenu: React.FC<EntryActionsMenuProps> = ({
  entryId,
  entryTitle,
  onDuplicate,
  onDelete,
  onEdit
}) => {
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);
  const [deleteDialogOpen, setDeleteDialogOpen] = useState(false);

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    event.stopPropagation();
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleEdit = () => {
    onEdit(entryId);
    handleMenuClose();
  };

  const handleDuplicate = () => {
    onDuplicate(entryId);
    handleMenuClose();
  };

  const handleDeleteClick = () => {
    setDeleteDialogOpen(true);
    handleMenuClose();
  };

  const handleDeleteConfirm = () => {
    onDelete(entryId);
    setDeleteDialogOpen(false);
  };

  return (
    <>
      <IconButton
        onClick={handleMenuOpen}
        size="small"
        aria-label="entry actions"
      >
        <MoreVertIcon />
      </IconButton>

      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={handleMenuClose}
      >
        <MenuItem onClick={handleEdit}>
          <ListItemIcon>
            <EditIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Edit</ListItemText>
        </MenuItem>

        <MenuItem onClick={handleDuplicate}>
          <ListItemIcon>
            <DuplicateIcon fontSize="small" />
          </ListItemIcon>
          <ListItemText>Duplicate</ListItemText>
        </MenuItem>

        <MenuItem onClick={handleDeleteClick}>
          <ListItemIcon>
            <DeleteIcon fontSize="small" color="error" />
          </ListItemIcon>
          <ListItemText>Delete</ListItemText>
        </MenuItem>
      </Menu>

      <Dialog open={deleteDialogOpen} onClose={() => setDeleteDialogOpen(false)}>
        <DialogTitle>Delete Entry</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Are you sure you want to delete "{entryTitle}"? This action cannot be undone.
          </DialogContent>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDeleteDialogOpen(false)}>Cancel</Button>
          <Button onClick={handleDeleteConfirm} color="error" variant="contained">
            Delete
          </Button>
        </DialogActions>
      </Dialog>
    </>
  );
};

export default EntryActionsMenu;
