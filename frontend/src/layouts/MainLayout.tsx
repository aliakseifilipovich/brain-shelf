import { Box, Toolbar, Snackbar, Alert } from '@mui/material';
import { Header } from './Header';
import { Sidebar } from './Sidebar';
import { useAppDispatch, useAppSelector } from '@store/hooks';
import { hideSnackbar } from '@store/uiSlice';

interface MainLayoutProps {
  children: React.ReactNode;
}

export const MainLayout = ({ children }: MainLayoutProps) => {
  const dispatch = useAppDispatch();
  const drawerOpen = useAppSelector(state => state.ui.drawerOpen);
  const snackbar = useAppSelector(state => state.ui.snackbar);

  const handleCloseSnackbar = () => {
    dispatch(hideSnackbar());
  };

  return (
    <Box sx={{ display: 'flex' }}>
      <Header />
      <Sidebar />
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          width: '100%',
          ml: drawerOpen ? 0 : '-240px',
          transition: theme =>
            theme.transitions.create(['margin'], {
              easing: theme.transitions.easing.sharp,
              duration: theme.transitions.duration.leavingScreen,
            }),
        }}
      >
        <Toolbar />
        {children}
      </Box>
      <Snackbar
        open={snackbar.open}
        autoHideDuration={6000}
        onClose={handleCloseSnackbar}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert onClose={handleCloseSnackbar} severity={snackbar.severity} sx={{ width: '100%' }}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
};
