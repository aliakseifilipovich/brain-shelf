# Frontend - Brain Shelf

React + TypeScript frontend for Brain Shelf application, built with Vite.

## Structure

```
frontend/
├── src/
│   ├── App.tsx        # Main application component
│   ├── main.tsx       # Application entry point
│   ├── App.css        # Application styles
│   └── index.css      # Global styles
├── public/            # Static assets
├── index.html         # HTML template
└── vite.config.ts     # Vite configuration
```

## Technologies

- **React 18** - UI library
- **TypeScript 5** - Type-safe JavaScript
- **Vite 5** - Fast build tool and dev server
- **React Router** - Client-side routing (to be added in future issues)

## Getting Started

### Run with Docker (Recommended)

From the project root:
```bash
docker-compose up -d
```

The frontend will be available at http://localhost:3000

### Run Locally

**Prerequisites**: Node.js 20+

```bash
# Install dependencies
npm install

# Start development server
npm run dev
```

The app will be available at http://localhost:3000 with hot reload enabled.

## Scripts

- `npm run dev` - Start development server with hot reload
- `npm run build` - Build for production
- `npm run preview` - Preview production build locally
- `npm run lint` - Run ESLint

## Features

### Current Features (Issue #1)
- ✅ Basic application layout
- ✅ Backend health check integration
- ✅ Responsive design
- ✅ Feature showcase cards
- ✅ Docker support with hot reload

### Planned Features
- Multi-project management UI (Issue #7)
- Entry creation and management (Issue #8)
- Full-text search interface (Issue #9)
- Multilingual support (Issue #10)
- Link metadata display (Issue #11)

## Environment Variables

Create a `.env` file in the frontend directory (optional):

```env
VITE_API_URL=http://localhost:5000
```

**Note**: Environment variables must be prefixed with `VITE_` to be accessible in the app.

## Development

### Hot Reload

The development server supports hot module replacement (HMR). Changes to `.tsx`, `.ts`, `.css` files will automatically update in the browser without losing component state.

### Type Safety

The project is configured with strict TypeScript settings:
- Strict null checks
- No implicit any
- Unused locals/parameters detection
- No fallthrough cases in switch

### Styling

The application uses vanilla CSS with:
- CSS custom properties for theming
- Responsive design with media queries
- Mobile-first approach
- Dark theme by default

## API Integration

The frontend communicates with the backend API at `http://localhost:5000` (configurable via `VITE_API_URL`).

Current API endpoints used:
- `GET /api/health` - Check backend status

## Building for Production

```bash
# Build the app
npm run build

# Preview the build
npm run preview
```

The build output will be in the `dist/` directory.

## Docker Support

The `Dockerfile` provides:
- Development mode with hot reload
- Volume mounting for live code updates
- Node.js 20 Alpine for smaller image size
- Port 3000 exposed for the dev server

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Troubleshooting

**Port already in use**:
```bash
# Change port in vite.config.ts or use environment variable
PORT=3001 npm run dev
```

**Dependencies not installing**:
```bash
# Clear npm cache and reinstall
rm -rf node_modules package-lock.json
npm install
```

**Hot reload not working in Docker**:
The Vite config includes `usePolling: true` for Docker compatibility. If issues persist, try rebuilding the container:
```bash
docker-compose down
docker-compose build --no-cache frontend
docker-compose up -d
```
