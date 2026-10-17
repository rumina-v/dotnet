param([int]$DatabasePort = 5433, [int]$ApiPort = 5147)

$ErrorActionPreference = 'Stop'
if ($DatabasePort -lt 1 -or $DatabasePort -gt 65535 -or $ApiPort -lt 1 -or $ApiPort -gt 65535) {
    throw 'Ports must be between 1 and 65535.'
}
Set-Location -LiteralPath $PSScriptRoot
$secretPath = Join-Path $PSScriptRoot 'local\postgres_password.txt'
if (-not (Test-Path -LiteralPath $secretPath)) { throw 'Run .\Start-Postgres.ps1 first.' }

$logDirectory = Join-Path $PSScriptRoot 'local\logs'
New-Item -ItemType Directory -Path $logDirectory -Force | Out-Null
$logPath = Join-Path $logDirectory 'api.jsonl'
$utf8 = New-Object System.Text.UTF8Encoding($false)

# Пароль остаётся в памяти процесса; файл в local/ исключён из Git.
$previousConnection = $env:ConnectionStrings__Users
try {
    $password = [IO.File]::ReadAllText($secretPath).Trim()
    $env:ConnectionStrings__Users = "Host=127.0.0.1;Port=$DatabasePort;Database=user_registration;Username=user_registration;Password=$password"
    dotnet run --project hw2_auth/UserRegistration.Api --no-launch-profile -- --urls "http://localhost:$ApiPort" |
        ForEach-Object {
            $line = [string]$_
            Write-Output $line
            $entry = $null
            if ($line.TrimStart().StartsWith('{')) {
                try { $entry = ConvertFrom-Json -InputObject $line -ErrorAction Stop } catch { }
            }
            if ($null -ne $entry -and $null -ne $entry.Timestamp -and $null -ne $entry.LogLevel) {
                [IO.File]::AppendAllText($logPath, $line + [Environment]::NewLine, $utf8)
            }
        }
    if ($LASTEXITCODE -ne 0) { throw 'API did not start successfully.' }
}
finally {
    $env:ConnectionStrings__Users = $previousConnection
    $password = $null
}
