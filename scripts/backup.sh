#!/bin/bash

# Database Backup Script for Brain Shelf
# This script creates automated backups of the PostgreSQL database

set -e

# Configuration
BACKUP_DIR="./backups"
DATE=$(date +%Y%m%d_%H%M%S)
BACKUP_FILE="$BACKUP_DIR/brainshelf_$DATE.sql"
POSTGRES_CONTAINER="brainshelf-db-prod"
POSTGRES_USER="${POSTGRES_USER:-admin}"
POSTGRES_DB="${POSTGRES_DB:-brainshelf}"
BACKUP_RETENTION_DAYS="${BACKUP_RETENTION_DAYS:-7}"

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Create backup directory if it doesn't exist
mkdir -p "$BACKUP_DIR"

echo -e "${YELLOW}Starting database backup...${NC}"

# Check if container is running
if ! docker ps | grep -q "$POSTGRES_CONTAINER"; then
    echo -e "${RED}Error: PostgreSQL container '$POSTGRES_CONTAINER' is not running${NC}"
    exit 1
fi

# Create backup
echo -e "Creating backup: ${BACKUP_FILE}"
if docker exec "$POSTGRES_CONTAINER" pg_dump -U "$POSTGRES_USER" "$POSTGRES_DB" > "$BACKUP_FILE"; then
    echo -e "${GREEN}✓ Backup created successfully${NC}"
else
    echo -e "${RED}✗ Backup failed${NC}"
    exit 1
fi

# Compress backup
echo -e "Compressing backup..."
if gzip "$BACKUP_FILE"; then
    echo -e "${GREEN}✓ Backup compressed: ${BACKUP_FILE}.gz${NC}"
else
    echo -e "${RED}✗ Compression failed${NC}"
    exit 1
fi

# Calculate backup size
BACKUP_SIZE=$(du -h "${BACKUP_FILE}.gz" | cut -f1)
echo -e "Backup size: ${BACKUP_SIZE}"

# Delete old backups
echo -e "${YELLOW}Cleaning up old backups (older than ${BACKUP_RETENTION_DAYS} days)...${NC}"
DELETED_COUNT=$(find "$BACKUP_DIR" -name "brainshelf_*.sql.gz" -mtime +${BACKUP_RETENTION_DAYS} -delete -print | wc -l)

if [ "$DELETED_COUNT" -gt 0 ]; then
    echo -e "${GREEN}✓ Deleted ${DELETED_COUNT} old backup(s)${NC}"
else
    echo -e "No old backups to delete"
fi

# List remaining backups
echo -e "\n${YELLOW}Current backups:${NC}"
ls -lh "$BACKUP_DIR"/*.gz 2>/dev/null || echo "No backups found"

# Backup summary
TOTAL_BACKUPS=$(ls "$BACKUP_DIR"/*.gz 2>/dev/null | wc -l)
TOTAL_SIZE=$(du -sh "$BACKUP_DIR" 2>/dev/null | cut -f1)

echo -e "\n${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "${GREEN}Backup completed successfully!${NC}"
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
echo -e "Total backups: ${TOTAL_BACKUPS}"
echo -e "Total size: ${TOTAL_SIZE}"
echo -e "Latest backup: ${BACKUP_FILE}.gz (${BACKUP_SIZE})"
echo -e "${GREEN}━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━${NC}"
