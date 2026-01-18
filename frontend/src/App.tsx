import { useState, useEffect } from 'react'
import './App.css'

interface HealthResponse {
  status: string;
  timestamp: string;
  service: string;
  version: string;
}

function App() {
  const [healthStatus, setHealthStatus] = useState<HealthResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const checkHealth = async () => {
      try {
        const apiUrl = import.meta.env.VITE_API_URL || 'http://localhost:5000';
        const response = await fetch(`${apiUrl}/api/health`);
        
        if (!response.ok) {
          throw new Error('Failed to fetch health status');
        }
        
        const data = await response.json();
        setHealthStatus(data);
        setError(null);
      } catch (err) {
        setError(err instanceof Error ? err.message : 'Unknown error');
      } finally {
        setLoading(false);
      }
    };

    checkHealth();
  }, []);

  return (
    <div className="app">
      <header className="app-header">
        <h1>🧠 Brain Shelf</h1>
        <p className="subtitle">Centralized Knowledge Management System</p>
      </header>

      <main className="app-main">
        <div className="status-card">
          <h2>System Status</h2>
          
          {loading && <p className="loading">Checking backend connection...</p>}
          
          {error && (
            <div className="error">
              <p>❌ Backend connection failed</p>
              <p className="error-message">{error}</p>
            </div>
          )}
          
          {healthStatus && (
            <div className="success">
              <p>✅ Backend is {healthStatus.status}</p>
              <div className="health-details">
                <p><strong>Service:</strong> {healthStatus.service}</p>
                <p><strong>Version:</strong> {healthStatus.version}</p>
                <p><strong>Time:</strong> {new Date(healthStatus.timestamp).toLocaleString()}</p>
              </div>
            </div>
          )}
        </div>

        <div className="features">
          <h2>Features</h2>
          <div className="feature-grid">
            <div className="feature-card">
              <span className="feature-icon">📁</span>
              <h3>Multi-Project Support</h3>
              <p>Organize information across multiple projects</p>
            </div>
            <div className="feature-card">
              <span className="feature-icon">🔍</span>
              <h3>Intelligent Search</h3>
              <p>Full-text search with filters and autocomplete</p>
            </div>
            <div className="feature-card">
              <span className="feature-icon">🔗</span>
              <h3>Link Metadata</h3>
              <p>Automatic extraction of page information</p>
            </div>
            <div className="feature-card">
              <span className="feature-icon">🌐</span>
              <h3>Multilingual</h3>
              <p>Support for English and Russian</p>
            </div>
          </div>
        </div>
      </main>

      <footer className="app-footer">
        <p>Brain Shelf v1.0.0 | Built with React + TypeScript + .NET</p>
      </footer>
    </div>
  )
}

export default App
