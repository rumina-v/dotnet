param([Parameter(Mandatory = $true)][ValidateRange(1, 2147483647)][int]$UserId, [int]$ApiPort = 5147)

$ErrorActionPreference = 'Stop'
if ($ApiPort -lt 1 -or $ApiPort -gt 65535) { throw 'ApiPort must be between 1 and 65535.' }
$baseUrl = "http://localhost:$ApiPort/api/requests"

foreach ($status in @('approved', 'rejected')) {
    $body = @{ userId = $UserId; title = "Demo $status"; description = 'Microservice demonstration' } |
        ConvertTo-Json -Compress
    $request = Invoke-RestMethod $baseUrl -Method Post -ContentType 'application/json' -Body $body
    Write-Host "Created $($request.id): $($request.status)"

    $decision = @{ status = $status } | ConvertTo-Json -Compress
    $result = Invoke-RestMethod "$baseUrl/$($request.id)/decision" -Method Put `
        -ContentType 'application/json' -Body $decision
    Write-Host "Decision: $($result.status), date: $($result.decidedDate)"

    try {
        Invoke-RestMethod "$baseUrl/$($request.id)/decision" -Method Put `
            -ContentType 'application/json' -Body $decision | Out-Null
        throw 'Expected a conflict for a second decision.'
    }
    catch {
        if ($null -eq $_.Exception.Response -or [int]$_.Exception.Response.StatusCode -ne 409) { throw }
        Write-Host 'Repeated decision: 409 (expected)'
    }
}

Invoke-RestMethod "$($baseUrl)?userId=$UserId" | Format-Table id, title, status, createdDate, decidedDate
