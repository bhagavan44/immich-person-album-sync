#!/bin/sh
set -euo pipefail

# Defaults
: "${SYNC_CRON:=0 * * * *}"
: "${SYNC_ARGS:=}"

# Ensure /var/spool/cron/crontabs exists
mkdir -p /var/spool/cron/crontabs

# Create crontab for root
# Send job output to container stdout so `docker logs` shows runs
CRON_LINE="${SYNC_CRON} /app/ImmichAlbumSync ${SYNC_ARGS} >> /proc/1/fd/1 2>&1"
echo "Installing cron: ${CRON_LINE}"
printf "%s\n" "${CRON_LINE}" | crontab -

# List crontab for visibility
crontab -l || true

# Start cron in foreground
exec crond -f -l 2
