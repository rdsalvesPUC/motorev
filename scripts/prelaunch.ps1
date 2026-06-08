[CmdletBinding()]
param(
    [switch]$SeedDatabase
)

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

    $seedEnabledByEnv = $env:MOTOREV_SEED_DATABASE -in @("1", "true", "TRUE", "True", "yes", "YES", "Yes")
    if ($SeedDatabase -or $seedEnabledByEnv) {
        Write-Host "Seeding database..."
        & (Join-Path $PSScriptRoot "seed-database.ps1") -SkipDocker
    }
    else {
        Write-Host "Skipping database seed. Use -SeedDatabase or MOTOREV_SEED_DATABASE=true to enable it."
    }
}
finally {
    Pop-Location
}
