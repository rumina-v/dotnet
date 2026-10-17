param([int]$ApiPort = 5147, [switch]$IncludeDatabaseError)

$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot
$baseUrl = "http://localhost:$ApiPort"
$login = 'demo_' + [guid]::NewGuid().ToString('N').Substring(0, 12)
$password = [guid]::NewGuid().ToString('N') + [guid]::NewGuid().ToString('N')
$createdId = $null

function Invoke-DemoRequest {
    param([string]$Method, [string]$Path, [int]$Expected, $Body = $null)
    $parameters = @{
        Uri = $baseUrl + $Path
        Method = $Method
        UseBasicParsing = $true
        TimeoutSec = 30
    }
    if ($null -ne $Body) {
        $parameters.ContentType = 'application/json; charset=utf-8'
        $parameters.Body = [Text.Encoding]::UTF8.GetBytes(($Body | ConvertTo-Json -Compress))
    }
    try {
        $response = Invoke-WebRequest @parameters
        $status = [int]$response.StatusCode
        $content = $response.Content
    }
    catch {
        if ($null -eq $_.Exception.Response) { throw }
        $status = [int]$_.Exception.Response.StatusCode
        $content = $null
    }
    if ($status -ne $Expected) { throw "$Method $Path returned $status, expected $Expected." }
    Write-Host "$Method $Path -> $status"
    if (-not [string]::IsNullOrWhiteSpace($content)) {
        return (ConvertFrom-Json -InputObject $content)
    }
}

try {
    $registration = @{ login = $login; password = $password; firstName = 'Demo'; lastName = 'User' }
    $user = Invoke-DemoRequest Post '/api/users' 201 $registration
    $createdId = $user.id
    Invoke-DemoRequest Post '/api/users' 409 $registration | Out-Null

    for ($i = 0; $i -lt 3; $i++) {
        Invoke-DemoRequest Post '/api/auth/login' 200 @{ login = $login; password = $password } | Out-Null
        Invoke-DemoRequest Post '/api/auth/login' 401 @{ login = $login; password = $password + 'wrong' } | Out-Null
        Start-Sleep -Milliseconds 500
    }
    Invoke-DemoRequest Put "/api/users/$createdId" 204 @{ firstName = 'Updated'; lastName = 'Demo' } | Out-Null
    Invoke-DemoRequest Get '/api/users' 200 | Out-Null
    Invoke-DemoRequest Get '/api/users/2147483647' 404 | Out-Null
    Invoke-DemoRequest Post '/api/users' 400 @{ login = ''; password = $password } | Out-Null

    if ($IncludeDatabaseError) {
        try {
            docker compose stop postgres
            if ($LASTEXITCODE -ne 0) { throw 'Could not stop the homework PostgreSQL container.' }
            Invoke-DemoRequest Get '/api/users' 500 | Out-Null
        }
        finally {
            docker compose up -d --wait postgres
            if ($LASTEXITCODE -ne 0) { throw 'Restart PostgreSQL with .\Start-Postgres.ps1.' }
        }
    }
}
finally {
    if ($null -ne $createdId) {
        Invoke-DemoRequest Delete "/api/users/$createdId" 204 | Out-Null
    }
    $password = $null
}

Write-Host 'Demo events sent. Wait a few seconds and refresh the dashboard.'
