#!/bin/bash
set -e

echo "=== VedAstro Railway Startup ==="
echo "[1/3] Starting Azurite (Azure Storage Emulator) in background..."

# Create data directory if it doesn't exist
mkdir -p /data/azurite

# Start Azurite in the background with loose mode (accepts any account key)
azurite \
  --loose \
  --blobHost 0.0.0.0 --blobPort 10000 \
  --queueHost 0.0.0.0 --queuePort 10001 \
  --tableHost 0.0.0.0 --tablePort 10002 \
  --location /data/azurite \
  --silent &

AZURITE_PID=$!
echo "[1/3] Azurite started (PID: $AZURITE_PID)"

echo "[2/3] Waiting for Azurite to be ready..."
sleep 3

# Quick health check â€” verify Table endpoint responds
for i in 1 2 3 4 5; do
  if curl -sf http://127.0.0.1:10002/ > /dev/null 2>&1; then
    echo "[2/3] Azurite is ready!"
    break
  fi
  echo "[2/3] Waiting... (attempt $i/5)"
  sleep 2
done

echo "[3/3] Starting Azure Functions Host (VedAstro API)..."

# Start the Azure Functions host â€” this is the main process
# When this exits, the container stops
exec /opt/startup/start_nonappservice.sh
