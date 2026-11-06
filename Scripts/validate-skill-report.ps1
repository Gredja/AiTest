param(
    [Parameter(Mandatory = $true)]
    [string]$Path
)

function Get-NumericLeaves {
    param($Node)

    $leaves = @()
    if ($null -eq $Node) { return $leaves }
    if ($Node -is [System.Management.Automation.PSCustomObject]) {
        foreach ($prop in $Node.PSObject.Properties) {
            $leaves += Get-NumericLeaves -Node $prop.Value
        }
        return $leaves
    }
    if ($Node -is [int] -or $Node -is [long] -or $Node -is [double] -or $Node -is [decimal]) {
        $leaves += [int]$Node
    }
    return $leaves
}

$issues = @()

try {
    $report = Get-Content -Path $Path -Raw | ConvertFrom-Json
}
catch {
    Write-Host "FAIL: JSON does not parse: $($_.Exception.Message)"
    exit 1
}

if ($null -eq $report -or $report -isnot [System.Management.Automation.PSCustomObject]) {
    Write-Host "FAIL: report is not a JSON object"
    exit 1
}

foreach ($key in @('files', 'tests', 'safetyCheck')) {
    if (-not ($report.PSObject.Properties.Name -contains $key)) {
        $issues += "missing required key: $key"
    }
}

if ($report.PSObject.Properties.Name -contains 'files') {
    foreach ($listKey in @('created', 'modified')) {
        if (-not ($report.files.PSObject.Properties.Name -contains $listKey)) {
            $issues += "missing required key: files.$listKey"
        }
        elseif ($report.files.$listKey -isnot [System.Array]) {
            $issues += "files.$listKey must be an array"
        }
    }
}

if ($report.PSObject.Properties.Name -contains 'tests') {
    if (-not ($report.tests.PSObject.Properties.Name -contains 'total')) {
        $issues += "missing required key: tests.total"
    }
    else {
        $counters = @()
        foreach ($prop in $report.tests.PSObject.Properties) {
            if ($prop.Name -eq 'total') { continue }
            $counters += Get-NumericLeaves -Node $prop.Value
        }
        $sum = ($counters | Measure-Object -Sum).Sum
        if ($null -eq $sum) { $sum = 0 }
        if ([int]$report.tests.total -ne [int]$sum) {
            $issues += "tests.total ($($report.tests.total)) != sum of counters ($sum)"
        }
    }
}

$format = $null
$failed = 0
$hasFormat = $false
if ($report.PSObject.Properties.Name -contains 'safetyCheck') {
    if (-not ($report.safetyCheck.PSObject.Properties.Name -contains 'format')) {
        $issues += "missing required key: safetyCheck.format"
    }
    else {
        $hasFormat = $true
        $format = [string]$report.safetyCheck.format
        if ($format -notin @('pass', 'fail')) {
            $issues += "safetyCheck.format must be exactly 'pass' or 'fail', got '$format'"
        }
    }
    if (-not ($report.safetyCheck.PSObject.Properties.Name -contains 'testRun')) {
        $issues += "missing required key: safetyCheck.testRun"
    }
    elseif ($report.safetyCheck.testRun.PSObject.Properties.Name -contains 'failed') {
        $failed = [int]$report.safetyCheck.testRun.failed
    }
    else {
        $issues += "missing required key: safetyCheck.testRun.failed"
    }
    if (-not ($report.safetyCheck.PSObject.Properties.Name -contains 'issues')) {
        $issues += "missing required key: safetyCheck.issues"
    }
}

$hasCommit = ($report.PSObject.Properties.Name -contains 'suggestedCommitMessage') -and
    -not [string]::IsNullOrWhiteSpace([string]$report.suggestedCommitMessage)

if ($hasFormat -and $format -eq 'pass' -and $failed -eq 0) {
    if (-not $hasCommit) {
        $issues += "green gate (format=pass, failed=0) requires a non-empty suggestedCommitMessage"
    }
}
elseif ($hasCommit) {
    $issues += "red gate (format!=pass or failed>0) forbids suggestedCommitMessage"
}

if ($issues.Count -eq 0) {
    Write-Host "PASS"
    exit 0
}

Write-Host "FAIL: $($issues.Count) issue(s):"
$issues | ForEach-Object { Write-Host "  - $_" }
exit 1
