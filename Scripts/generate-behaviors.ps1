# Scripts/generate-behaviors.ps1
# Generate behaviors.json from NUnit categories in allure-results

param(
    [string]$ResultsDir,
    [string]$ReportDataDir
)

$ErrorActionPreference = "Stop"

if (-not $ResultsDir -or -not $ReportDataDir) {
    $ProjectRoot = Split-Path $PSScriptRoot -Parent
    $ResultsDir = Join-Path $ProjectRoot "allure-results"
    $ReportDataDir = Join-Path $ProjectRoot "allure-report\data"
}

$PrimaryCategories = @("Smoke", "Validation", "Negative", "EdgeCase", "Performance")

$resultFiles = Get-ChildItem $ResultsDir -Filter "*result*.json"
$tests = @()

foreach ($file in $resultFiles) {
    $json = Get-Content $file.FullName | ConvertFrom-Json
    $tagLabels = $json.labels | Where-Object { $_.name -eq "tag" }
    $tags = @($tagLabels | ForEach-Object { $_.value })

    $primaryTag = $tags | Where-Object { $_ -in $PrimaryCategories } | Select-Object -First 1
    if (-not $primaryTag) { $primaryTag = "Other" }

    $speedTag = $tags | Where-Object { $_ -in @("Fast", "Slow") } | Select-Object -First 1
    if (-not $speedTag) { $speedTag = "Normal" }

    $tests += [PSCustomObject]@{
        Name   = $json.name
        Uid    = $json.uuid
        Status = $json.status
        Tags   = $tags
        PrimaryCategory = $primaryTag
        SpeedCategory   = $speedTag
    }
}

$tree = @{
    uid = "nunit-categories"
    name = "behaviors"
    children = @()
}

foreach ($primary in $PrimaryCategories) {
    $primaryTests = $tests | Where-Object { $_.PrimaryCategory -eq $primary }
    if ($primaryTests.Count -eq 0) { continue }

    $primaryNode = @{
        name = $primary
        children = @()
    }

    $speedGroups = $primaryTests | Group-Object SpeedCategory
    foreach ($group in $speedGroups) {
        $speedNode = @{
            name = $group.Name
            children = @()
        }

        foreach ($test in $group.Group) {
            $speedNode.children += @{
                name = $test.Name
                uid = $test.Uid
                status = $test.Status
                time = @{}
                flaky = $false
                newFailed = $false
                newPassed = $false
                newBroken = $false
                retriesCount = 0
                retriesStatusChange = $false
                parameters = @()
                tags = $test.Tags
            }
        }

        $primaryNode.children += $speedNode
    }

    $tree.children += $primaryNode
}

$otherTests = $tests | Where-Object { $_.PrimaryCategory -eq "Other" }
if ($otherTests.Count -gt 0) {
    $otherNode = @{
        name = "Other"
        children = @()
    }
    foreach ($test in $otherTests) {
        $otherNode.children += @{
            name = $test.Name
            uid = $test.Uid
            status = $test.Status
            time = @{}
            flaky = $false
            newFailed = $false
            newPassed = $false
            newBroken = $false
            retriesCount = 0
            retriesStatusChange = $false
            parameters = @()
            tags = $test.Tags
        }
    }
    $tree.children += $otherNode
}

$behaviorsPath = Join-Path $ReportDataDir "behaviors.json"
$tree | ConvertTo-Json -Depth 10 | Set-Content $behaviorsPath -Encoding UTF8

# Generate categories.json with correct format (no extra nesting)
$categoriesTree = @{
    uid = "nunit-status-categories"
    name = "categories"
    children = @()
}

$statusMap = @{
    "Passed" = "passed"
    "Skipped" = "skipped"
    "Broken" = @("broken", "failed")
}

foreach ($statusName in @("Passed", "Skipped", "Broken")) {
    $matchedStatuses = $statusMap[$statusName]
    $statusTests = $tests | Where-Object {
        if ($matchedStatuses -is [array]) {
            $_.Status -in $matchedStatuses
        } else {
            $_.Status -eq $matchedStatuses
        }
    }

    $categoryNode = @{
        name = $statusName
        uid = "category-$($statusName.ToLower())"
        children = @()
    }

    foreach ($test in $statusTests) {
        $categoryNode.children += @{
            name = $test.Name
            uid = $test.Uid
            status = $test.Status
            time = @{}
            flaky = $false
            newFailed = $false
            newPassed = $false
            newBroken = $false
            retriesCount = 0
            retriesStatusChange = $false
            parameters = @()
            tags = $test.Tags
        }
    }

    $categoriesTree.children += $categoryNode
}

$categoriesPath = Join-Path $ReportDataDir "categories.json"
$categoriesTree | ConvertTo-Json -Depth 10 | Set-Content $categoriesPath -Encoding UTF8
Write-Host "Generated behaviors.json with $($tests.Count) tests and categories.json" -ForegroundColor Green
