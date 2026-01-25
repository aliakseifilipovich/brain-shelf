#!/bin/bash

# Database Restore Script for Brain Shelf
# This script restores the PostgreSQL database from a backup file

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Configuration
POSTGRES_CONTAINER="brainshelf-db-prod"
POSTGRES_USER="${POSTGRES_USER:-admin}"
POSTGRES_DB="${POSTGRES_DB:-brainshelf}"
COMPOSE_FILE="docker-compose.prod.yml"

# Check if backup file is provided
if [ -z "$1" ]; then
    echo -e "${RED}Error: No backup file specified${NC}"
    echo -e "Usage: $0 <backup_file.sql.gz>"
    echo -e "\nAvailable backups:"
    ls -lh ./backups/*.gz 2>/dev/null || echo "No backups found"
    exit 1
fi

BACKUP_FILE="$1"

# Check if backup file exists
if [ ! -f "$BACKUP_FILE" ]; then
    echo -e "${RED}Error: Backup file '$BACKUP_FILE' not found${NC}"
    exit 1
fi

echo -e "${YELLOW}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${YELLOW}Database Restore${NC}"
echo -e "${YELLOW}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "Backup file: ${BACKUP_FILE}"
echo -e "Database: ${POSTGRES_DB}"
echo -e "Container: ${POSTGRES_CONTAINER}"
echo -e "${YELLOW}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"

# Confirmation
read -p "Are you sure you want to restore? This will overwrite the current database. (yes/no): " -r
echo
if [[ ! $REPLY =~ ^[Yy]es$ ]]; then
    echo -e "${YELLOW}Restore cancelled${NC}"
    exit 0
fi

# Check if container is running
if ! docker ps | grep -q "$POSTGRES_CONTAINER"; then
    echo -e "${RED}Error: PostgreSQL container '$POSTGRES_CONTAINER' is not running${NC}"
    echo -e "Start it with: docker-compose -f $COMPOSE_FILE up -d database"
    exit 1
fi

# Stop backend to prevent database connections
echo -e "${YELLOW}Stopping backend service...${NC}"
docker-compose -f "$COMPOSE_FILE" stop backend || true

# Wait a moment for connections to close
sleep 2

# Drop existing connections
echo -e "${YELLOW}Dropping existing connections...${NC}"
docker exec "$POSTGRES_CONTAINER" psql -U "$POSTGRES_USER" -d postgres -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$POSTGRES_DB' AND pid <> pg_backend_pid();" || true

# Restore from backup
echo -e "${YELLOW}Restoring database from backup...${NC}"
if gunzip -c "$BACKUP_FILE" | docker exec -i "$POSTGRES_CONTAINER" psql -U "$POSTGRES_USER" -d "$POSTGRES_DB"; then
    echo -e "${GREEN}✓ Database restored successfully${NC}"
else
    echo -e "${RED}✗ Restore failed${NC}"
    echo -e "${YELLOW}Starting backend service...${NC}"
    docker-compose -f "$COMPOSE_FILE" start backend
    exit 1
fi

# Restart backend service
echo -e "${YELLOW}Starting backend service...${NC}"
docker-compose -f "$COMPOSE_FILE" start backend

# Wait for backend to be healthy
echo -e "${YELLOW}Waiting for backend to be healthy...${NC}"
sleep 5

# Check backend health
if docker-compose -f "$COMPOSE_FILE" ps | grep backend | grep -q "healthy"; then
    echo -e "${GREEN}✓ Backend is healthy${NC}"
else
    echo -e "${YELLOW}⚠ Backend health check pending (this may take a moment)${NC}"
fi

echo -e "\n${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${GREEN}Restore completed successfully!${NC}"
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
