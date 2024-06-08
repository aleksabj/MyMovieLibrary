#!/bin/bash

MYSQL_USER="root"
MYSQL_PASSWORD="Jurnalist1"

# Create database
mysql -u $MYSQL_USER -p$MYSQL_PASSWORD -e "CREATE DATABASE IF NOT EXISTS MovieDB;"
# Import database dump
mysql -u $MYSQL_USER -p$MYSQL_PASSWORD MovieDB < ./database/database_dump.sql

echo "Database setup completed!"
