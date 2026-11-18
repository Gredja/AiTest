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

function Get-ReportStats {
    param([string]$Path)

    if ([string]::IsNullOrEmpty($Path)) {
        return $null
    }

    $reportFiles = @()
    if (Test-Path $Path -PathType Container) {
        $reportFiles = @(Get-ChildItem -Path $Path -Filter 'TestRunReport-*.md' | Sort-Object Name)
    }
    elseif (Test-Path $Path -PathType Leaf) {
        $reportFiles = @(Get-Item $Path)
    }

    if ($reportFiles.Count -eq 0) {
        return $null
    }

    # Two-pass semantics: reports are merged chronologically (name = yyyyMMdd-HHmmss),
    # a later report (rerun pass) overrides the same test from an earlier one.
    $tests = [ordered]@{}
    $summary = @{}

    foreach ($file in $reportFiles) {
        foreach ($line in Get-Content -Path $file.FullName) {
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

            $name = $cells[2].Trim()
            if ([string]::IsNullOrEmpty($name)) {
                continue
            }

            $key = "$($cells[3].Trim())::$name"
            $tests[$key] = @{ Category = $cells[4].Trim(); Result = $cells[5].Trim() }
        }
    }

    if ($tests.Count -eq 0) {
        if ($summary.Count -eq 0) {
            return $null
        }
        return @{ Summary = $summary; Categories = @{} }
    }

    $summaryResult = @{ Passed = 0; Failed = 0; Skipped = 0 }
    $categories = @{}
    foreach ($entry in $tests.Values) {
        $result = if ($entry.Result -eq 'Passed' -or $entry.Result -eq 'Failed') { $entry.Result } else { 'Skipped' }
        $summaryResult[$result]++

        $category = $entry.Category
        if ([string]::IsNullOrEmpty($category)) {
            continue
        }
        if (-not $categories.ContainsKey($category)) {
            $categories[$category] = @{ Passed = 0; Failed = 0; Total = 0 }
        }
        $categories[$category].Total++
        if ($entry.Result -eq 'Passed') {
            $categories[$category].Passed++
        }
        elseif ($entry.Result -eq 'Failed') {
            $categories[$category].Failed++
        }
    }

    return @{ Summary = $summaryResult; Categories = $categories }
}

function Get-StatusStyle {
    param([string]$RunStatus)

    switch ($RunStatus) {
        'success' { return @{ Emoji = '✅'; Color = '#2eb67d' } }
        'failure' { return @{ Emoji = '❌'; Color = '#e01e5a' } }
        'cancelled' { return @{ Emoji = '⏹'; Color = '#7c3085' } }
        default { return @{ Emoji = '⚠️'; Color = '#ecb22e' } }
    }
}

function Format-RunMessage {
    param(
        [string]$TitleText,
        [string]$RunStatus,
        [string]$Url,
        [string]$EventName,
        [hashtable]$Stats
    )

    $statusEmoji = (Get-StatusStyle -RunStatus $RunStatus).Emoji

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

function Format-RunBlocks {
    param(
        [string]$TitleText,
        [string]$RunStatus,
        [string]$Url,
        [string]$EventName,
        [hashtable]$Stats
    )

    $style = Get-StatusStyle -RunStatus $RunStatus

    $runNumber = if (-not [string]::IsNullOrEmpty($env:GITHUB_RUN_NUMBER)) {
        " #$($env:GITHUB_RUN_NUMBER)"
    }
    else {
        [string]::Empty
    }

    $when = (Get-Date).ToUniversalTime().ToString('yyyy-MM-dd HH:mm')

    $blocks = @()
    $blocks += [ordered]@{
        type = 'header'
        text = [ordered]@{ type = 'plain_text'; text = "$($style.Emoji) $TitleText$runNumber"; emoji = $true }
    }
    $blocks += [ordered]@{
        type = 'section'
        text = [ordered]@{ type = 'mrkdwn'; text = "*Когда:* $when UTC · *Event:* $EventName" }
    }

    if ($null -ne $Stats) {
        $passed = $Stats.Summary['Passed']
        $failed = $Stats.Summary['Failed']
        $skipped = $Stats.Summary['Skipped']
        if ($null -eq $skipped) { $skipped = 0 }
        $total = $passed + $failed + $skipped
        $percent = if ($total -gt 0) { [math]::Round(100.0 * $passed / $total, 1) } else { 0 }

        $summaryLine = "*Итог:* $passed passed / $failed failed / $skipped skipped — $total ($percent%)"
        if ($Stats.Categories.Count -gt 0) {
            $parts = $Stats.Categories.Keys | Sort-Object | ForEach-Object {
                $entry = $Stats.Categories[$_]
                $text = "$_ $($entry.Passed)/$($entry.Total)"
                if ($entry.Failed -gt 0) {
                    $text += " ($($entry.Failed)❌)"
                }
                $text
            }
            $summaryLine += "`n*Категории:* $($parts -join ' · ')"
        }
        $blocks += [ordered]@{ type = 'section'; text = [ordered]@{ type = 'mrkdwn'; text = $summaryLine } }
    }
    elseif ($RunStatus -ne 'success') {
        $blocks += [ordered]@{
            type = 'section'
            text = [ordered]@{ type = 'mrkdwn'; text = "*Статус:* $RunStatus — подробности в логах рана" }
        }
    }

    $blocks += [ordered]@{
        type = 'actions'
        elements = @(
            [ordered]@{
                type = 'button'
                text = [ordered]@{ type = 'plain_text'; text = 'Открыть ран'; emoji = $true }
                url = $Url
            }
        )
    }

    return @{ Blocks = $blocks; Color = $style.Color }
}

function Send-SlackMessage {
    param(
        [string]$Text,
        [object[]]$Blocks,
        [string]$Color
    )

    $message = [ordered]@{ channel = $env:SLACK_CHANNEL; text = $Text }
    if ($null -ne $Blocks -and $Blocks.Count -gt 0) {
        $message['attachments'] = @(
            [ordered]@{ color = $Color; blocks = @($Blocks) }
        )
    }
    $payload = $message | ConvertTo-Json -Compress -Depth 8
    $bodyBytes = [Text.UTF8Encoding]::new($false).GetBytes($payload)
    $result = Invoke-RestMethod -Uri 'https://slack.com/api/chat.postMessage' `
        -Method Post `
        -Headers @{ Authorization = "Bearer $($env:SLACK_BOT_TOKEN)" } `
        -ContentType 'application/json; charset=utf-8' `
        -Body $bodyBytes
    if (-not $result.ok) {
        Write-Warning "Slack post failed: $($result.error)"
    }
    else {
        Write-Output "Slack message posted ts=$($result.ts) channel=$($result.channel)"
    }
}

try {
    if ([string]::IsNullOrEmpty($env:SLACK_BOT_TOKEN) -or [string]::IsNullOrEmpty($env:SLACK_CHANNEL)) {
        Write-Warning 'SLACK_BOT_TOKEN/SLACK_CHANNEL environment variables not set — notification skipped'
        exit 0
    }

    $stats = Get-ReportStats -Path $ReportPath
    $message = Format-RunMessage -TitleText $Title -RunStatus $Status -Url $RunUrl -EventName $Event -Stats $stats
    $rich = Format-RunBlocks -TitleText $Title -RunStatus $Status -Url $RunUrl -EventName $Event -Stats $stats
    Send-SlackMessage -Text $message -Blocks $rich.Blocks -Color $rich.Color
}
catch {
    Write-Warning "Slack notification failed: $($_.Exception.Message)"
}
exit 0
