param([int]$DatabasePort = 5434, [int]$ApiPort = 5157)

$ErrorActionPreference = 'Stop'
if ($DatabasePort -lt 1 -or $DatabasePort -gt 65535 -or $ApiPort -lt 1 -or $ApiPort -gt 65535) {
    throw 'Ports must be between 1 and 65535.'
}
Set-Location -LiteralPath $PSScriptRoot
$localDirectory = Join-Path $PSScriptRoot 'local'
New-Item -ItemType Directory -Path $localDirectory -Force | Out-Null
$secretPath = Join-Path $localDirectory 'approvals_password.txt'
if (-not (Test-Path -LiteralPath $secretPath)) {
    $bytes = New-Object byte[] 32
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) } finally { $rng.Dispose() }
    [IO.File]::WriteAllText($secretPath, [Convert]::ToBase64String($bytes))
}

$previousPort = $env:APPROVALS_POSTGRES_PORT
$previousConnection = $env:ConnectionStrings__Approvals
try {
    $env:APPROVALS_POSTGRES_PORT = $DatabasePort.ToString()
    docker compose --profile microservices up -d --wait approvals-postgres
    if ($LASTEXITCODE -ne 0) { throw 'Approvals PostgreSQL did not start. Check Docker Desktop.' }
    $password = [IO.File]::ReadAllText($secretPath).Trim()
    $env:ConnectionStrings__Approvals = "Host=127.0.0.1;Port=$DatabasePort;Database=approvals;Username=approvals;Password=$password"
    dotnet run --project hw4_microservices/Approvals.Api --no-launch-profile -- --urls "http://localhost:$ApiPort"
    if ($LASTEXITCODE -ne 0) { throw 'Approvals API exited with an error.' }
}
finally {
    $env:APPROVALS_POSTGRES_PORT = $previousPort
    $env:ConnectionStrings__Approvals = $previousConnection
    $password = $null
}
