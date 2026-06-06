$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$solution = Join-Path $repoRoot "MotoRev.sln"

Push-Location $repoRoot
try {
    Write-Host "Starting Docker infrastructure..."
    docker compose up -d

    Write-Host "Running unit tests..."
    dotnet test $solution

    Write-Host "Building solution..."
    dotnet build $solution --no-restore
}
finally {
    Pop-Location
}
