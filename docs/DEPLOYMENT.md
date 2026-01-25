# Production Deployment Guide

This guide provides instructions for deploying Brain Shelf to a production environment using Docker.

## Table of Contents
- [Prerequisites](#prerequisites)
- [Configuration](#configuration)
- [Deployment](#deployment)
- [Health Checks](#health-checks)
- [Backup and Restore](#backup-and-restore)
- [Monitoring](#monitoring)
- [Troubleshooting](#troubleshooting)
- [Security Best Practices](#security-best-practices)

## Prerequisites

- Docker Engine 20.10+
- Docker Compose 2.0+
- Minimum 2GB RAM
- 10GB disk space

## Configuration

### 1. Environment Variables

Create production environment file:

```bash
cp .env.prod.example .env.prod
```

Edit `.env.prod` and set secure values:

```bash
# Database Configuration
POSTGRES_DB=brainshelf
POSTGRES_USER=admin
POSTGRES_PASSWORD=your_secure_password_here  # CHANGE THIS!
POSTGRES_PORT=5432

# Frontend Configuration
FRONTEND_PORT=80
FRONTEND_SSL_PORT=443
VITE_API_URL=http://your-domain.com/api  # or http://localhost/api for local

# Backup Configuration
BACKUP_RETENTION_DAYS=7
```

**Important**: 
- Change `POSTGRES_PASSWORD` to a strong, random password
- Update `VITE_API_URL` to match your domain or server IP
- Never commit `.env.prod` to version control

### 2. SSL/TLS Configuration (Optional but Recommended)

For production deployments, configure SSL/TLS:

1. Obtain SSL certificates (Let's Encrypt, commercial CA, etc.)
2. Place certificates in `./frontend/ssl/`:
   - `cert.pem` - SSL certificate
   - `key.pem` - Private key
3. Update nginx configuration to enable HTTPS

## Deployment

### Quick Start

1. **Build and start all services**:
   ```bash
   docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d
   ```

2. **Check service status**:
   ```bash
   docker-compose -f docker-compose.prod.yml ps
   ```

3. **View logs**:
   ```bash
   # All services
   docker-compose -f docker-compose.prod.yml logs -f

   # Specific service
   docker-compose -f docker-compose.prod.yml logs -f backend
   ```

4. **Access the application**:
   - Frontend: `http://localhost` (or your configured domain)
   - API: `http://localhost/api`

### Detailed Deployment Steps

#### 1. Prepare the Server

```bash
# Update system packages
sudo apt update && sudo apt upgrade -y

# Install Docker and Docker Compose if not already installed
curl -fsSL https://get.docker.com -o get-docker.sh
sudo sh get-docker.sh

# Add current user to docker group (requires logout/login)
sudo usermod -aG docker $USER
```

#### 2. Clone Repository

```bash
git clone https://github.com/aliakseifilipovich/brain-shelf.git
cd brain-shelf
```

#### 3. Configure Environment

```bash
cp .env.prod.example .env.prod
nano .env.prod  # Edit with your values
```

#### 4. Build Images

```bash
docker-compose -f docker-compose.prod.yml build
```

#### 5. Start Services

```bash
docker-compose -f docker-compose.prod.yml --env-file .env.prod up -d
```

#### 6. Apply Database Migrations

The migrations run automatically on startup. Verify with:

```bash
docker-compose -f docker-compose.prod.yml logs backend | grep "migration"
```

#### 7. Verify Deployment

Check all services are healthy:

```bash
docker-compose -f docker-compose.prod.yml ps
```

All services should show status `Up (healthy)`.

## Health Checks

All services include health checks:

### Database Health Check
```bash
docker exec brainshelf-db-prod pg_isready -U admin -d brainshelf
```

### Backend Health Check
```bash
curl http://localhost/api/health
```

### Frontend Health Check
```bash
curl http://localhost/health
```

## Backup and Restore

### Database Backup

#### Automated Backup Script

Create `backup.sh`:

```bash
#!/bin/bash
BACKUP_DIR="./backups"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/brainshelf_$DATE.sql"

# Create backup directory if it doesn't exist
mkdir -p $BACKUP_DIR

# Create backup
docker exec brainshelf-db-prod pg_dump -U admin brainshelf > $BACKUP_FILE

# Compress backup
gzip $BACKUP_FILE

# Delete backups older than BACKUP_RETENTION_DAYS
find $BACKUP_DIR -name "brainshelf_*.sql.gz" -mtime +${BACKUP_RETENTION_DAYS:-7} -delete

echo "Backup completed: ${BACKUP_FILE}.gz"
```

Make it executable and set up cron:

```bash
chmod +x backup.sh

# Add to crontab (daily at 2 AM)
crontab -e
# Add: 0 2 * * * /path/to/brain-shelf/backup.sh
```

#### Manual Backup

```bash
# Create backup
docker exec brainshelf-db-prod pg_dump -U admin brainshelf > backup_$(date +%Y%m%d).sql

# Compress
gzip backup_$(date +%Y%m%d).sql
```

### Database Restore

```bash
# Stop backend to prevent connections
docker-compose -f docker-compose.prod.yml stop backend

# Restore from backup
gunzip -c backup_file.sql.gz | docker exec -i brainshelf-db-prod psql -U admin -d brainshelf

# Restart services
docker-compose -f docker-compose.prod.yml start backend
```

## Monitoring

### View Service Logs

```bash
# All services (follow mode)
docker-compose -f docker-compose.prod.yml logs -f

# Specific service
docker-compose -f docker-compose.prod.yml logs -f backend

# Last 100 lines
docker-compose -f docker-compose.prod.yml logs --tail=100
```

### Resource Usage

```bash
# Monitor container resources
docker stats

# Specific container
docker stats brainshelf-backend-prod
```

### Log Rotation

Logs are automatically rotated with:
- Maximum size: 10MB
- Maximum files: 3
- Total max space per container: 30MB

## Stopping and Restarting

### Stop All Services
```bash
docker-compose -f docker-compose.prod.yml down
```

### Stop Without Removing Containers
```bash
docker-compose -f docker-compose.prod.yml stop
```

### Restart Services
```bash
docker-compose -f docker-compose.prod.yml restart
```

### Update and Redeploy
```bash
# Pull latest code
git pull origin main

# Rebuild and restart
docker-compose -f docker-compose.prod.yml up -d --build
```

## Troubleshooting

### Service Won't Start

1. **Check logs**:
   ```bash
   docker-compose -f docker-compose.prod.yml logs [service-name]
   ```

2. **Check if port is already in use**:
   ```bash
   sudo lsof -i :80  # Frontend
   sudo lsof -i :5432  # Database
   ```

3. **Verify environment variables**:
   ```bash
   docker-compose -f docker-compose.prod.yml config
   ```

### Database Connection Issues

1. **Verify database is healthy**:
   ```bash
   docker exec brainshelf-db-prod pg_isready -U admin
   ```

2. **Check connection string** in backend logs
3. **Verify network connectivity**:
   ```bash
   docker network inspect brain-shelf_brainshelf-network
   ```

### Frontend Not Loading

1. **Check nginx logs**:
   ```bash
   docker-compose -f docker-compose.prod.yml logs frontend
   ```

2. **Verify API proxy is working**:
   ```bash
   curl http://localhost/api/health
   ```

3. **Check browser console** for JavaScript errors

### Performance Issues

1. **Check resource usage**:
   ```bash
   docker stats
   ```

2. **Review resource limits** in `docker-compose.prod.yml`
3. **Check database performance**:
   ```bash
   docker exec brainshelf-db-prod psql -U admin -d brainshelf -c "SELECT * FROM pg_stat_activity;"
   ```

## Security Best Practices

### 1. Use Strong Passwords
- Generate random passwords: `openssl rand -base64 32`
- Change default credentials immediately

### 2. Restrict Network Access
- Use firewall rules to limit access
- Only expose necessary ports
- Consider using a reverse proxy (nginx, Traefik)

### 3. Enable SSL/TLS
- Use Let's Encrypt for free certificates
- Configure HTTPS in nginx
- Redirect HTTP to HTTPS

### 4. Regular Updates
- Keep Docker and Docker Compose updated
- Update base images regularly
- Apply security patches promptly

### 5. Secure Environment Files
```bash
chmod 600 .env.prod
```

### 6. Non-Root Containers
All containers run as non-root users (UID 1000)

### 7. Resource Limits
Resource limits prevent DoS attacks and resource exhaustion

### 8. Log Management
- Regularly review logs
- Set up log aggregation (ELK, Grafana Loki)
- Monitor for suspicious activity

### 9. Database Security
- Regular backups
- Limit database exposure (not publicly accessible)
- Use connection pooling
- Enable SSL for database connections (optional)

### 10. Network Isolation
Services communicate through isolated Docker network

## Maintenance

### Regular Tasks

**Daily**:
- Review logs for errors
- Check service health
- Monitor disk space

**Weekly**:
- Review resource usage
- Check for security updates
- Test backup restoration

**Monthly**:
- Update Docker images
- Review and rotate logs
- Performance optimization

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [PostgreSQL Best Practices](https://www.postgresql.org/docs/current/index.html)
- [Nginx Documentation](https://nginx.org/en/docs/)
- [.NET Production Deployment](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/)

## Support

For issues and questions:
- GitHub Issues: https://github.com/aliakseifilipovich/brain-shelf/issues
- Documentation: See README.md

## License

See LICENSE file in repository root.
