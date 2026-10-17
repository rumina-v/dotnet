param([int]$Port = 5433)

$ErrorActionPreference = 'Stop'
if ($Port -lt 1 -or $Port -gt 65535) { throw 'Port must be between 1 and 65535.' }
Set-Location -LiteralPath $PSScriptRoot

$localDirectory = Join-Path $PSScriptRoot 'local'
$secretPath = Join-Path $localDirectory 'postgres_password.txt'
New-Item -ItemType Directory -Path $localDirectory -Force | Out-Null
if (-not (Test-Path -LiteralPath $secretPath)) {
    $bytes = New-Object byte[] 32
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    [IO.File]::WriteAllText($secretPath, [Convert]::ToBase64String($bytes))
}

$env:POSTGRES_PORT = $Port.ToString()
docker compose up -d --wait postgres
if ($LASTEXITCODE -ne 0) { throw 'PostgreSQL did not start. Check that Docker Desktop is running.' }
Write-Host "PostgreSQL is ready on 127.0.0.1:$Port."
