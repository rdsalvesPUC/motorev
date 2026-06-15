[CmdletBinding()]
param(
    [string]$Server = "localhost,1433",
    [string]$Database = "MotoRevDb",
    [string]$User = "sa",
    [string]$Password = "SuaSenhaForte123!",
    [switch]$SkipDocker,
    [switch]$SkipMigrations
)

$ErrorActionPreference = "Stop"

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot "..")
$apiProject = Join-Path $repoRoot "server\MotoRevApi\MotoRevApi.csproj"
$startupProject = Join-Path $repoRoot "server\MotoRevApi\MotoRevApi.csproj"

$seedScripts = @(
    "seed-pecas-servicos.sql",
    "seed-linhas.sql",
    "seed-modelos-motos.sql",
    "seed-revisoes-padrao.sql",
    "seed-clientes-motos.sql",
    "seed-concessionarias.sql",
    "seed-agendamentos.sql"
)

function Invoke-SqlScript {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Path
    )

    $sqlcmd = Get-Command sqlcmd -ErrorAction SilentlyContinue
    if ($sqlcmd) {
        & $sqlcmd.Source -S $Server -d $Database -U $User -P $Password -C -b -i $Path
        if ($LASTEXITCODE -ne 0) {
            throw "sqlcmd failed for script: $Path"
        }

        return
    }

    $docker = Get-Command docker -ErrorAction SilentlyContinue
    if (-not $docker) {
        throw "Neither sqlcmd nor docker was found. Install sqlcmd or run the SQL scripts manually."
    }

    $containerName = "motorev-sql-server"
    $containerSqlCmd = $null
    foreach ($candidate in @("/opt/mssql-tools18/bin/sqlcmd", "/opt/mssql-tools/bin/sqlcmd")) {
        & docker exec $containerName test -x $candidate *> $null
        if ($LASTEXITCODE -eq 0) {
            $containerSqlCmd = $candidate
            break
        }
    }

    if (-not $containerSqlCmd) {
        throw "sqlcmd was not found locally or inside container '$containerName'."
    }

    Get-Content -Raw -Path $Path | & docker exec -i $containerName $containerSqlCmd -S localhost -d $Database -U $User -P $Password -C -b
    if ($LASTEXITCODE -ne 0) {
        throw "docker sqlcmd failed for script: $Path"
    }
}

Push-Location $repoRoot
try {
    if (-not $SkipDocker) {
        Write-Host "Starting Docker infrastructure..."
        docker compose up -d
    }

    if (-not $SkipMigrations) {
        Write-Host "Applying EF Core migrations..."
        dotnet ef database update --project $apiProject --startup-project $startupProject --context AppDbContext
    }

    foreach ($script in $seedScripts) {
        $path = Join-Path $PSScriptRoot $script
        Write-Host "Running seed script: $script"
        Invoke-SqlScript -Path $path
    }

    Write-Host "Database seed completed."
}
finally {
    Pop-Location
}
