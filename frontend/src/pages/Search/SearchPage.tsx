import { Container, Typography, Box } from '@mui/material';

export const SearchPage = () => {
  return (
    <Container maxWidth="lg">
      <Box sx={{ mt: 4, mb: 4 }}>
        <Typography variant="h4" component="h1" gutterBottom>
          Search
        </Typography>
        <Typography variant="body1" color="text.secondary" paragraph>
          Search through your projects and entries. This page will include advanced search filters
          and result highlighting.
        </Typography>
      </Box>
    </Container>
  );
};
