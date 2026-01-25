import { useState } from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  IconButton,
  Chip,
  Box,
  Typography,
  TablePagination,
  Tooltip,
} from '@mui/material';
import EditIcon from '@mui/icons-material/Edit';
import DeleteIcon from '@mui/icons-material/Delete';
import VisibilityIcon from '@mui/icons-material/Visibility';
import { Entry, EntryType } from '@types/models';
import { useNavigate } from 'react-router-dom';

interface EntriesTableProps {
  entries: Entry[];
  totalCount: number;
  page: number;
  rowsPerPage: number;
  onPageChange: (newPage: number) => void;
  onRowsPerPageChange: (newRowsPerPage: number) => void;
  onEdit: (entry: Entry) => void;
  onDelete: (entry: Entry) => void;
  loading?: boolean;
}

const getTypeColor = (type: EntryType): string => {
  switch (type) {
    case EntryType.Link:
      return '#3B82F6'; // Blue
    case EntryType.Note:
      return '#10B981'; // Green
    case EntryType.Setting:
      return '#F59E0B'; // Amber
    case EntryType.Instruction:
      return '#8B5CF6'; // Purple
    default:
      return '#6B7280'; // Gray
  }
};

const getTypeLabel = (type: EntryType): string => {
  switch (type) {
    case EntryType.Link:
      return 'Link';
    case EntryType.Note:
      return 'Note';
    case EntryType.Setting:
      return 'Setting';
    case EntryType.Instruction:
      return 'Instruction';
    default:
      return 'Unknown';
  }
};

export const EntriesTable = ({
  entries,
  totalCount,
  page,
  rowsPerPage,
  onPageChange,
  onRowsPerPageChange,
  onEdit,
  onDelete,
  loading = false,
}: EntriesTableProps) => {
  const navigate = useNavigate();

  const handleChangePage = (_event: unknown, newPage: number) => {
    onPageChange(newPage);
  };

  const handleChangeRowsPerPage = (event: React.ChangeEvent<HTMLInputElement>) => {
    onRowsPerPageChange(parseInt(event.target.value, 10));
  };

  const handleView = (entry: Entry) => {
    navigate(`/entries/${entry.id}`);
  };

  const truncateText = (text: string | undefined | null, maxLength: number): string => {
    if (!text) return '';
    return text.length > maxLength ? `${text.substring(0, maxLength)}...` : text;
  };

  return (
    <Paper sx={{ width: '100%', overflow: 'hidden' }}>
      <TableContainer sx={{ maxHeight: 600 }}>
        <Table stickyHeader>
          <TableHead>
            <TableRow>
              <TableCell sx={{ fontWeight: 600, bgcolor: 'background.paper' }}>Title</TableCell>
              <TableCell sx={{ fontWeight: 600, bgcolor: 'background.paper', width: 100 }}>Type</TableCell>
              <TableCell sx={{ fontWeight: 600, bgcolor: 'background.paper', maxWidth: 300 }}>Content</TableCell>
              <TableCell sx={{ fontWeight: 600, bgcolor: 'background.paper', width: 200 }}>Tags</TableCell>
              <TableCell sx={{ fontWeight: 600, bgcolor: 'background.paper', width: 150 }}>Created</TableCell>
              <TableCell sx={{ fontWeight: 600, bgcolor: 'background.paper', width: 150 }}>Updated</TableCell>
              <TableCell sx={{ fontWeight: 600, bgcolor: 'background.paper', width: 130 }} align="center">Actions</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                  <Typography color="text.secondary">Loading...</Typography>
                </TableCell>
              </TableRow>
            ) : entries.length === 0 ? (
              <TableRow>
                <TableCell colSpan={7} align="center" sx={{ py: 4 }}>
                  <Typography color="text.secondary">No entries found</Typography>
                </TableCell>
              </TableRow>
            ) : (
              entries.map((entry) => (
                <TableRow
                  key={entry.id}
                  hover
                  sx={{
                    '&:hover': {
                      bgcolor: 'action.hover',
                      cursor: 'pointer',
                    },
                  }}
                >
                  <TableCell onClick={() => handleView(entry)}>
                    <Typography variant="body2" sx={{ fontWeight: 500 }}>
                      {entry.title}
                    </Typography>
                    {entry.description && (
                      <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.5 }}>
                        {truncateText(entry.description, 60)}
                      </Typography>
                    )}
                  </TableCell>
                  <TableCell onClick={() => handleView(entry)}>
                    <Chip
                      label={getTypeLabel(entry.type)}
                      size="small"
                      sx={{
                        bgcolor: getTypeColor(entry.type),
                        color: 'white',
                        fontWeight: 500,
                        fontSize: '0.7rem',
                        height: 20,
                      }}
                    />
                  </TableCell>
                  <TableCell onClick={() => handleView(entry)}>
                    <Typography variant="body2" color="text.secondary" sx={{ fontSize: '0.8rem' }}>
                      {truncateText(entry.content, 100)}
                    </Typography>
                  </TableCell>
                  <TableCell onClick={() => handleView(entry)}>
                    <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                      {entry.tags && entry.tags.length > 0 ? (
                        <>
                          {entry.tags.slice(0, 3).map((tag, index) => (
                            <Chip
                              key={index}
                              label={tag}
                              size="small"
                              variant="outlined"
                              sx={{
                                height: 20,
                                fontSize: '0.7rem',
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
                                height: 20,
                                fontSize: '0.7rem',
                                borderColor: 'divider',
                              }}
                            />
                          )}
                        </>
                      ) : (
                        <Typography variant="caption" color="text.disabled">
                          No tags
                        </Typography>
                      )}
                    </Box>
                  </TableCell>
                  <TableCell onClick={() => handleView(entry)}>
                    <Typography variant="caption" color="text.secondary" sx={{ fontSize: '0.75rem' }}>
                      {new Date(entry.createdAt).toLocaleDateString()}
                    </Typography>
                  </TableCell>
                  <TableCell onClick={() => handleView(entry)}>
                    <Typography variant="caption" color="text.secondary" sx={{ fontSize: '0.75rem' }}>
                      {new Date(entry.updatedAt).toLocaleDateString()}
                    </Typography>
                  </TableCell>
                  <TableCell align="center">
                    <Box sx={{ display: 'flex', gap: 0.5, justifyContent: 'center' }}>
                      <Tooltip title="View">
                        <IconButton
                          size="small"
                          onClick={(e) => {
                            e.stopPropagation();
                            handleView(entry);
                          }}
                          sx={{ fontSize: 18 }}
                        >
                          <VisibilityIcon fontSize="inherit" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Edit">
                        <IconButton
                          size="small"
                          onClick={(e) => {
                            e.stopPropagation();
                            onEdit(entry);
                          }}
                          sx={{ fontSize: 18 }}
                        >
                          <EditIcon fontSize="inherit" />
                        </IconButton>
                      </Tooltip>
                      <Tooltip title="Delete">
                        <IconButton
                          size="small"
                          onClick={(e) => {
                            e.stopPropagation();
                            onDelete(entry);
                          }}
                          sx={{ fontSize: 18, color: 'error.main' }}
                        >
                          <DeleteIcon fontSize="inherit" />
                        </IconButton>
                      </Tooltip>
                    </Box>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>
      <TablePagination
        rowsPerPageOptions={[10, 20, 50, 100]}
        component="div"
        count={totalCount}
        rowsPerPage={rowsPerPage}
        page={page}
        onPageChange={handleChangePage}
        onRowsPerPageChange={handleChangeRowsPerPage}
        sx={{
          borderTop: 1,
          borderColor: 'divider',
        }}
      />
    </Paper>
  );
};
