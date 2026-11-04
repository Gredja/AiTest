param(
    [Parameter(Mandatory = $true)]
    [string]$Title,

    [Parameter(Mandatory = $true)]
    [string]$Status,

    [Parameter(Mandatory = $true)]
    [string]$RunUrl,

    [Parameter(Mandatory = $true)]
    [string]$Event,

    [string]$ReportPath
)

$ErrorActionPreference = 'Stop'

function Get-SlackCredentials {
    if (-not [string]::IsNullOrEmpty($env:SLACK_BOT_TOKEN) -and -not [string]::IsNullOrEmpty($env:SLACK_CHANNEL)) {
        return
    }

    $repoRoot = Split-Path -Parent $PSScriptRoot
    $envFile = Join-Path $repoRoot '.env'
    if (-not (Test-Path $envFile)) {
        return
    }

    Select-String -Path $envFile -Pattern '^(SLACK_BOT_TOKEN|SLACK_CHANNEL)=(.*)$' | ForEach-Object {
        $name = $_.Matches[0].Groups[1].Value
        $value = $_.Matches[0].Groups[2].Value
        Set-Item -Path "Env:$name" -Value $value
    }
}

function Get-ReportStats {
    param([string]$Path)

    if ([string]::IsNullOrEmpty($Path)) {
        return $null
    }

    $reportFile = $Path
    if (Test-Path $Path -PathType Container) {
        $reportFile = Get-ChildItem -Path $Path -Filter 'TestRunReport-*.md' |
            Sort-Object LastWriteTime -Descending |
            Select-Object -First 1 -ExpandProperty FullName
    }

    if ([string]::IsNullOrEmpty($reportFile) -or -not (Test-Path $reportFile)) {
        return $null
    }

    $summary = @{}
    $categories = @{}

    foreach ($line in Get-Content -Path $reportFile) {
        if ($line -match '^\|\s*(Passed|Failed|Skipped)\s*\|\s*(\d+)\s*\|') {
            $summary[$Matches[1]] = [int]$Matches[2]
            continue
        }

        if ($line -notmatch '^\|\s*\d+\s*\|') {
            continue
        }

        $cells = $line -split '\|'
        if ($cells.Count -lt 7) {
            continue
        }

        $category = $cells[4].Trim()
        $result = $cells[5].Trim()
        if ([string]::IsNullOrEmpty($category)) {
            continue
        }

        if (-not $categories.ContainsKey($category)) {
            $categories[$category] = @{ Passed = 0; Failed = 0; Total = 0 }
        }
        $categories[$category].Total++
        if ($result -eq 'Passed') {
            $categories[$category].Passed++
        }
        elseif ($result -eq 'Failed') {
            $categories[$category].Failed++
        }
    }

    if ($summary.Count -eq 0) {
        return $null
    }

    return @{ Summary = $summary; Categories = $categories }
}

function Format-RunMessage {
    param(
        [string]$TitleText,
        [string]$RunStatus,
        [string]$Url,
        [string]$EventName,
        [hashtable]$Stats
    )

    $statusEmoji = switch ($RunStatus) {
        'success' { '✅' }
        'failure' { '❌' }
        'cancelled' { '⏹' }
        default { '⚠️' }
    }

    $runNumber = if (-not [string]::IsNullOrEmpty($env:GITHUB_RUN_NUMBER)) {
        " #$($env:GITHUB_RUN_NUMBER)"
    }
    else {
        [string]::Empty
    }

    $when = (Get-Date).ToUniversalTime().ToString('yyyy-MM-dd HH:mm')
    $lines = @("$TitleText$runNumber $statusEmoji", "Когда: $when UTC · $EventName")

    if ($null -ne $Stats) {
        $passed = $Stats.Summary['Passed']
        $failed = $Stats.Summary['Failed']
        $skipped = $Stats.Summary['Skipped']
        if ($null -eq $skipped) { $skipped = 0 }
        $total = $passed + $failed + $skipped
        $percent = if ($total -gt 0) { [math]::Round(100.0 * $passed / $total, 1) } else { 0 }
        $lines += "Итог: $passed passed / $failed failed / $skipped skipped — $total ($percent%)"

        if ($Stats.Categories.Count -gt 0) {
            $parts = $Stats.Categories.Keys | Sort-Object | ForEach-Object {
                $entry = $Stats.Categories[$_]
                $text = "$_ $($entry.Passed)/$($entry.Total)"
                if ($entry.Failed -gt 0) {
                    $text += " ($($entry.Failed)❌)"
                }
                $text
            }
            $lines += "Категории: $($parts -join ' · ')"
        }
    }
    elseif ($RunStatus -ne 'success') {
        $lines += "Статус: $RunStatus — подробности в логах рана"
    }

    $lines += "Отчёт: $Url"
    return ($lines -join "`n")
}

function Send-SlackMessage {
    param([string]$Text)

    $payload = @{ channel = $env:SLACK_CHANNEL; text = $Text } | ConvertTo-Json -Compress -Depth 3
    $payloadFile = [IO.Path]::GetTempFileName()
    try {
        [IO.File]::WriteAllText($payloadFile, $payload, [Text.UTF8Encoding]::new($false))
        $response = curl.exe -s -X POST 'https://slack.com/api/chat.postMessage' `
            -H "Authorization: Bearer $($env:SLACK_BOT_TOKEN)" `
            -H 'Content-Type: application/json' `
            --data-binary "@$payloadFile"
        $result = $response | ConvertFrom-Json
        if (-not $result.ok) {
            Write-Warning "Slack post failed: $($result.error)"
        }
        else {
            Write-Output "Slack message posted ts=$($result.ts) channel=$($result.channel)"
        }
    }
    finally {
        Remove-Item -Path $payloadFile -ErrorAction SilentlyContinue
    }
}

try {
    Get-SlackCredentials

    if ([string]::IsNullOrEmpty($env:SLACK_BOT_TOKEN) -or [string]::IsNullOrEmpty($env:SLACK_CHANNEL)) {
        Write-Warning 'SLACK_BOT_TOKEN/SLACK_CHANNEL not available — notification skipped'
        exit 0
    }

    $stats = Get-ReportStats -Path $ReportPath
    $message = Format-RunMessage -TitleText $Title -RunStatus $Status -Url $RunUrl -EventName $Event -Stats $stats
    Send-SlackMessage -Text $message
}
catch {
    Write-Warning "Slack notification failed: $($_.Exception.Message)"
}
exit 0
