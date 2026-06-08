#!/usr/bin/env zsh
set -euo pipefail

script_dir=${0:A:h}
repo_root=${script_dir:h}

if command -v pwsh >/dev/null 2>&1; then
  exec pwsh -NoProfile -ExecutionPolicy Bypass -File "$repo_root/scripts/prelaunch.ps1"
fi

cd "$repo_root"

echo "Starting Docker infrastructure..."
docker compose up -d

echo "Running unit tests..."
dotnet test "$repo_root/MotoRev.sln"

echo "Building solution..."
dotnet build "$repo_root/MotoRev.sln" --no-restore
