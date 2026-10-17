$ErrorActionPreference = 'Stop'
Set-Location -LiteralPath $PSScriptRoot

New-Item -ItemType Directory -Path (Join-Path $PSScriptRoot 'local\logs') -Force | Out-Null
docker compose --profile observability up -d --wait --wait-timeout 240 opensearch dashboards
if ($LASTEXITCODE -ne 0) { throw 'OpenSearch/Dashboards did not become ready. Inspect docker compose logs.' }

$openSearch = 'http://127.0.0.1:9200'
$dashboards = 'http://127.0.0.1:5601'
$headers = @{ 'osd-xsrf' = 'true' }
$template = [IO.File]::ReadAllText((Join-Path $PSScriptRoot 'observability\index-template.json'))
Invoke-RestMethod -Method Put -Uri "$openSearch/_index_template/user-registration-logs" `
    -ContentType 'application/json' -Body ([Text.Encoding]::UTF8.GetBytes($template)) | Out-Null

try {
    Invoke-WebRequest -UseBasicParsing -Method Head -Uri "$openSearch/user-registration-logs" | Out-Null
}
catch {
    if ($null -eq $_.Exception.Response -or [int]$_.Exception.Response.StatusCode -ne 404) { throw }
    Invoke-RestMethod -Method Put -Uri "$openSearch/user-registration-logs" `
        -ContentType 'application/json' -Body '{}' | Out-Null
}

foreach ($line in [IO.File]::ReadLines((Join-Path $PSScriptRoot 'observability\dashboards.ndjson'))) {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    $saved = ConvertFrom-Json -InputObject $line
    $body = @{
        attributes = $saved.attributes
        references = @($saved.references)
    } | ConvertTo-Json -Depth 50 -Compress
    Invoke-RestMethod -Method Post -Uri "$dashboards/api/saved_objects/$($saved.type)/$($saved.id)?overwrite=true" `
        -Headers $headers -ContentType 'application/json' -Body ([Text.Encoding]::UTF8.GetBytes($body)) | Out-Null
}

Invoke-RestMethod -Method Post -Uri "$dashboards/api/opensearch-dashboards/settings" `
    -Headers $headers -ContentType 'application/json' `
    -Body '{"changes":{"defaultIndex":"users-api-logs"}}' | Out-Null

docker compose --profile observability up -d fluent-bit
if ($LASTEXITCODE -ne 0) { throw 'Fluent Bit did not start.' }
Write-Host 'Dashboard: http://localhost:5601/app/dashboards#/view/users-api-dashboard'
Write-Host 'Start the API with .\Start-Api.ps1, then run .\Demo-Logs.ps1.'
