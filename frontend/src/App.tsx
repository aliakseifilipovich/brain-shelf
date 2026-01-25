import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { Provider } from 'react-redux';
import { ThemeProvider } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import { store } from '@store/index';
import { theme } from '@styles/theme';
import { MainLayout } from '@layouts/MainLayout';
import { ProjectsPage } from '@pages/Projects/ProjectsPage';
import { ProjectDetailPage } from '@pages/Projects/ProjectDetailPage';
import { EntriesPage } from '@pages/Entries/EntriesPage';
import { EntryDetailPage } from '@pages/Entries/EntryDetailPage';
import { SearchPage } from '@pages/Search/SearchPage';
import TagManagement from './pages/TagManagement';

function App() {
  return (
    <Provider store={store}>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <Router>
          <MainLayout>
            <Routes>
              <Route path="/" element={<Navigate to="/projects" replace />} />
              <Route path="/projects" element={<ProjectsPage />} />
              <Route path="/projects/:id" element={<ProjectDetailPage />} />
              <Route path="/entries" element={<EntriesPage />} />
              <Route path="/entries/:id" element={<EntryDetailPage />} />
              <Route path="/search" element={<SearchPage />} />
              <Route path="/tags" element={<TagManagement />} />
            </Routes>
          </MainLayout>
        </Router>
      </ThemeProvider>
    </Provider>
  );
}

export default App;
